using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
            var apiClient = new ApiClient(config, new HttpClient());
            
            while (true)
            {
                var tokenSource = new CancellationTokenSource();

                var tasks = CreateTasks(
                    config.TargetRPS,
                    () => apiClient.CallApiEndpointAsync(tokenSource.Token).Result);

                tokenSource.CancelAfter(1000);

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception)
                {
                    // ignored
                }

                var succesfulTasks = tasks.Count(task => task.Status == TaskStatus.RanToCompletion);

                var rps = (float)succesfulTasks / tasks.Count() * 100;
                Console.WriteLine($"Current RPS: {rps}");
            }
        }

        private static IEnumerable<Task<T>> CreateTasks<T>(uint numberOfTasks, Func<T> func)
        {
            var tasks = new List<Task<T>>();

            for (var i = 0; i < numberOfTasks; i++)
            {
                var newTask = new Task<T>(func);
                newTask.Start();
                tasks.Add(newTask);
            }
            
            return tasks;
        }
    }
    }
