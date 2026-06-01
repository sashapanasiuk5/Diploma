using System;
using System.Data.SqlClient; // Using legacy driver
using System.Threading.Tasks;

namespace FaultToleranceTest
{
    class Program
    {
        // Connection string to the DAM proxy
        private const string DamConnectionString = "Server=127.0.0.1,14330;Database=DummyDB;User Id=webadmin;Password=123456;TrustServerCertificate=True;Encrypt=False;";
        
        // Test Parameters
        private const int PingIntervalMs = 500;  // Delay between queries (500ms = 2 queries/sec)
        private const int TotalIterations = 100; // Define 'N' (Total number of queries to send)

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Fault Tolerance Test ===");
            Console.WriteLine($"Iterations: {TotalIterations} | Ping Interval: {PingIntervalMs} ms\n");

            int successCount = 0;
            int failCount = 0;
            bool isOnline = true;
            bool firstRun = true;

            Console.WriteLine("--> Starting periodic queries...\n");

            // Replaced infinite loop with a fixed N-iteration loop
            for (int i = 1; i <= TotalIterations; i++)
            {
                try
                {
                    // Create a new connection for each query to test the proxy's listener
                    using (var connection = new SqlConnection(DamConnectionString))
                    {
                        await connection.OpenAsync();
                        
                        // Lightweight query to check availability
                        using (var command = new SqlCommand("SELECT 1", connection))
                        {
                            await command.ExecuteScalarAsync();
                        }
                    }

                    successCount++;

                    // If system was offline but just responded - log the recovery
                    if (!isOnline || firstRun)
                    {
                        Console.WriteLine($"\n[ {DateTime.Now:HH:mm:ss} ] [ OK ] SYSTEM ONLINE AND RESPONDING.");
                        isOnline = true;
                        firstRun = false;
                    }
                }
                catch (Exception ex)
                {
                    failCount++;

                    // If system was online but just failed - log the fault
                    if (isOnline || firstRun)
                    {
                        Console.WriteLine($"\n[ {DateTime.Now:HH:mm:ss} ] [ FAULT ] CONNECTION LOST!");
                        Console.WriteLine($"    Reason: {ex.Message}");
                        isOnline = false;
                        firstRun = false;
                    }
                }

                // Dynamic console status update (\r returns cursor to the start of the line)
                string statusTag = isOnline ? "[ ONLINE ] " : "[ OFFLINE ]";
                Console.Write($"\rStep {i,3}/{TotalIterations} | Status: {statusTag} | Success: {successCount} | Fail: {failCount}      ");

                // Wait before the next iteration (unless it's the very last one)
                if (i < TotalIterations)
                {
                    await Task.Delay(PingIntervalMs);
                }
            }

            // Print final summary when the loop finishes
            Console.WriteLine("\n\n=======================================================");
            Console.WriteLine("                 TEST COMPLETED                        ");
            Console.WriteLine("=======================================================");
            Console.WriteLine($"Total Queries Attempted:  {TotalIterations}");
            Console.WriteLine($"Total Successful Queries: {successCount}");
            Console.WriteLine($"Total Failed Queries:     {failCount}");
            
            double availability = (double)successCount / TotalIterations * 100;
            Console.WriteLine($"Availability Score:       {availability:F2}%");
            Console.WriteLine("=======================================================\n");
        }
    }
}