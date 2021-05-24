using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using mParticle.LoadGenerator.Services;

namespace mParticle.LoadGenerator
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var configFile = "config.json";
            if (args.Length > 0)
            {
                configFile = args[0];
            }

            var config = Config.GetArguments(configFile);
            if (config == null)
            {
                Console.WriteLine("Failed to parse configuration.");
                return;
            }

            await RunLoad(config);
        }

        private static async Task RunLoad(Config config)
        {
            var targetRpsReached = false;
            var apiClient = new ApiClient(config, new HttpClient());
            var currentRps = 1;
            var currentPower = 1;

            LogStatus(currentRps, config.TargetRPS);

            while (true)
            {
                var numberOfTasks = config.SecondsPerRate * currentRps;

                var tasks = CreateTasks(
                    numberOfTasks,
                    () => apiClient.CallApiEndpointAsync().Result);

                foreach (var task in tasks)
                {
                    try
                    {
                        task.Start();
                        Console.WriteLine($"task {task.Id} started");
                        await Task.Delay(1000 / currentRps);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.Write($"Error - ${e.Message}");
                    }
                }

                currentPower++;
                currentRps = Math.Min((int)config.TargetRPS, (int)Math.Pow(2, currentPower));

                if (IsEndOfExecution(config, currentRps, targetRpsReached))
                {
                    return;
                }

                if (HasReachedTargetRps(config, currentRps))
                {
                    targetRpsReached = true;
                }

                await Task.WhenAll(tasks);

                var successfulTasksCount = tasks.Count(task => task.Status == TaskStatus.RanToCompletion);
                Console.WriteLine($"Tasks executed successfully: {successfulTasksCount}");

                var failedTasks = tasks.Where(task => task.Status == TaskStatus.Faulted);
                Console.WriteLine($"Tasks failed: {failedTasks.Count()}");

                Console.WriteLine();
                LogStatus(currentRps, config.TargetRPS);
            }
        }

        private static bool HasReachedTargetRps(Config config, int currentRps)
        {
            return currentRps == config.TargetRPS;
        }

        private static bool IsEndOfExecution(Config config, int currentRps, bool targetRpsReached)
        {
            return HasReachedTargetRps(config, currentRps) && targetRpsReached;
        }

        private static IEnumerable<Task<T>> CreateTasks<T>(int numberOfTasks, Func<T> func)
        {
            var tasks = new List<Task<T>>();
            for (var i = 0; i < numberOfTasks; i++)
            {
                tasks.Add(new Task<T>(func));
            }

            return tasks;
        }

        private static void LogStatus(int currentRps, uint targetRps)
        {
            Console.WriteLine($"Current RPS: {currentRps}\t\t Target RPS: {targetRps}");
        }
    }
}
