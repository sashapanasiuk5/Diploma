using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient; // Використовуємо legacy драйвер для сумісності з проксі

namespace LatencyTest
{
    class Program
    {
        // Рядки підключення (Pooling увімкнено за замовчуванням)
        private const string BareConnectionString = "Server=127.0.0.1,1433;Database=DummyDB;User Id=webadmin;Password=123456;TrustServerCertificate=True;Encrypt=False;";
        private const string DamConnectionString  = "Server=127.0.0.1,14330;Database=DummyDB;User Id=webadmin;Password=123456;TrustServerCertificate=True;Encrypt=False;";

        // --- ПАРАМЕТРИ ІМІТАЦІЙНОГО МОДЕЛЮВАННЯ ---
        private const int IndependentReplications = 20; // Кількість незалежних експериментів (N)
        private const int TestIterations = 500;         // Час моделювання (T_mod) одного експерименту
        private const int WarmupIterations = 150;        // Перехідний період (Warmup)

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== System latency experimental test ===");
            Console.WriteLine($"Method: Independent Replications (Replications: {IndependentReplications}, Length: {TestIterations})\n");

            // 1. Дослідження базової системи (Bare)
            var bareResults = await RunExperimentReplicationsAsync("Bare System", BareConnectionString);

            // 2. Дослідження DAM системи (Gallium)
            var damResults = await RunExperimentReplicationsAsync("Monitored System", DamConnectionString);

            // 3. Статистичний звіт (порівняння вибірок)
            PrintStatisticalReport(bareResults, damResults);
        }

        static async Task<List<double>> RunExperimentReplicationsAsync(string systemName, string connectionString)
        {
            Console.WriteLine($"\n--> Starting experiment series for: {systemName}");
            var replicationOutputs = new List<double>(IndependentReplications);

            for (int r = 1; r <= IndependentReplications; r++)
            {
                // Відкидаємо дані перехідного періоду перед КОЖНИМ прогоном
                for (int i = 0; i < WarmupIterations; i++)
                {
                    await ExecuteDatabaseQueryAsync(connectionString);
                }

                // Стабілізація ОС та пам'яті для чистоти експерименту
                using (Process p = Process.GetCurrentProcess()) { p.PriorityClass = ProcessPriorityClass.High; }
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();

                // Запуск одного імітаційного прогону
                var runLatencies = new List<double>(TestIterations);
                var stopwatch = new Stopwatch();

                for (int i = 0; i < TestIterations; i++)
                {
                    stopwatch.Restart();
                    await ExecuteDatabaseQueryAsync(connectionString);
                    stopwatch.Stop();
                    runLatencies.Add(stopwatch.Elapsed.TotalMilliseconds);
                }

                using (Process p = Process.GetCurrentProcess()) { p.PriorityClass = ProcessPriorityClass.Normal; }

                // Формуємо вихід моделі для поточного прогону (використовуємо 95-й перцентиль для стабільності)
                var sorted = runLatencies.OrderBy(x => x).ToList();
                int p95Index = (int)Math.Ceiling(95 / 100.0 * sorted.Count) - 1;
                double p95Output = sorted[p95Index];
                
                replicationOutputs.Add(p95Output);
                Console.WriteLine($"    Run {r,2}/{IndependentReplications} | Model Output (P95): {p95Output:F2} ms");
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

        static void PrintStatisticalReport(List<double> bare, List<double> dam)
        {
            // Calculate grand means (Mean of means)
            double bareMean = bare.Average();
            double damMean = dam.Average();

            // Calculate standard deviation (σ) for the samples
            double bareStdDev = CalculateStandardDeviation(bare);
            double damStdDev = CalculateStandardDeviation(dam);

            double delta = damMean - bareMean;

            Console.WriteLine("\n=======================================================");
            Console.WriteLine("             STATISTICAL REPORT               ");
            Console.WriteLine("=======================================================");
            
            Console.WriteLine($"{"Metric",-25} | {"Bare System",-15} | {"Monitored System",-15}");
            Console.WriteLine(new string('-', 60));
            
            Console.WriteLine($"{"Mean",-25} | {bareMean,7:F2} ms      | {damMean,7:F2} ms");
            Console.WriteLine($"{"Standard Deviation",-25} | {bareStdDev,7:F3}         | {damStdDev,7:F3}");
            
            Console.WriteLine("\n-------------------------------------------------------");
            Console.WriteLine("                    RESEARCH CONCLUSION                ");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine($"Absolute Overhead: {delta:F2} ms");
            Console.WriteLine("=======================================================\n");
            
            if (bareStdDev > delta)
            {
                Console.WriteLine("WARNING: Baseline system standard deviation is greater than the delta!");
                Console.WriteLine("This means system noise (variance) overshadows the DAM overhead.");
                Console.WriteLine("Resolution: Increase IndependentReplications or TestIterations.\n");
            }
        }

        static double CalculateStandardDeviation(List<double> values)
        {
            double avg = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - avg) * (val - avg)).Sum();
            // Використовуємо N-1 для вибіркового стандартного відхилення
            return Math.Sqrt(sumOfSquaresOfDifferences / (values.Count - 1));
        }
    }
}