using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient; // Using legacy driver for proxy compatibility

namespace ThroughputTest
{
    class Program
    {
        // Connection String (Pointing strictly to the DAM Proxy)
        private const string ConnectionString = "Server=127.0.0.1,14330;Database=DummyDB;User Id=webadmin;Password=123456;TrustServerCertificate=True;Encrypt=False;";

        // --- SIMULATION MODELING PARAMETERS ---
        private const int IndependentReplications = 20;    // Number of independent load tests (N)
        private const int RequestsPerReplication = 2000;  // Simulation time (T_mod) per experiment
        private const int ConcurrencyLevel = 30;          // Concurrent threads hitting the proxy

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== DAM Throughput Experimental test ===");
            Console.WriteLine($"Target: Monitored System");
            Console.WriteLine($"Method: Independent Replications (Replications: {IndependentReplications}, Requests/Run: {RequestsPerReplication}, Concurrency: {ConcurrencyLevel})\n");

            // Run DAM System Study
            var damResults = await RunThroughputReplicationsAsync("Monitored System", ConnectionString);

            // Print Final Report
            PrintStatisticalReport(damResults);
        }

        static async Task<List<double>> RunThroughputReplicationsAsync(string systemName, string connectionString)
        {
            Console.WriteLine($"\n--> Starting throughput experiment series for: {systemName}");
            var replicationOutputs = new List<double>(IndependentReplications);

            for (int r = 1; r <= IndependentReplications; r++)
            {
                // Warmup: Establish connection pool for the concurrent threads
                for (int i = 0; i < ConcurrencyLevel * 10; i++)
                {
                    try { await ExecuteDatabaseQueryAsync(connectionString); } catch { /* Ignore warmup errors */ }
                }

                // OS and Memory stabilization for experiment purity
                using (Process p = Process.GetCurrentProcess()) { p.PriorityClass = ProcessPriorityClass.High; }
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();

                int successCount = 0;
                int errorCount = 0;
                var semaphore = new SemaphoreSlim(ConcurrencyLevel);
                var tasks = new List<Task>(RequestsPerReplication);
                
                var stopwatch = Stopwatch.StartNew();

                // Run concurrent workload for this replication
                for (int i = 0; i < RequestsPerReplication; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await ExecuteDatabaseQueryAsync(connectionString);
                            Interlocked.Increment(ref successCount); // Thread-safe counter
                        }
                        catch (Exception)
                        {
                            Interlocked.Increment(ref errorCount);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                // Wait for all queries in this run to complete
                await Task.WhenAll(tasks);
                stopwatch.Stop();

                using (Process p = Process.GetCurrentProcess()) { p.PriorityClass = ProcessPriorityClass.Normal; }

                // Calculate TPS for this specific run
                double totalSeconds = stopwatch.Elapsed.TotalSeconds;
                double tpsOutput = successCount / totalSeconds;
                
                replicationOutputs.Add(tpsOutput);
                
                string errorLog = errorCount > 0 ? $" | Errors: {errorCount}" : "";
                Console.WriteLine($"    Run {r,2}/{IndependentReplications} | Model Output: {tpsOutput:F0} TPS{errorLog}");
            }

            return replicationOutputs;
        }

        static async Task ExecuteDatabaseQueryAsync(string connectionString)
        {
            int randomUserId = Random.Shared.Next(1, 21);

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT o.order_id, o.order_date, p.name AS product_name, oi.quantity, oi.unit_price
                    FROM dbo.orders o
                    JOIN dbo.order_items oi ON o.order_id = oi.order_id
                    JOIN dbo.products p ON oi.product_id = p.product_id
                    WHERE o.user_id = @UserId;";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", randomUserId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var oId = reader.GetInt32(0);
                            var date = reader.GetDateTime(1);
                        }
                    }
                }
            }
        }

        static void PrintStatisticalReport(List<double> results)
        {
            // Calculate grand mean and standard deviation
            double mean = results.Average();
            double stdDev = CalculateStandardDeviation(results);

            Console.WriteLine("\n=======================================================");
            Console.WriteLine("             STATISTICAL REPORT            ");
            Console.WriteLine("=======================================================");
            
            Console.WriteLine($"{"Metric",-25} | {"Result",-15}");
            Console.WriteLine(new string('-', 45));
            
            Console.WriteLine($"{"Grand Mean",-25} | {mean,7:F0} req/s");
            Console.WriteLine($"{"Standard Deviation",-25} | {stdDev,7:F1}");
            Console.WriteLine("=======================================================\n");
           
        }

        static double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count <= 1) return 0;
            double avg = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - avg) * (val - avg)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / (values.Count - 1));
        }
    }
}