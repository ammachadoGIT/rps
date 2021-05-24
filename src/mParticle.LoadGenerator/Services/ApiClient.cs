using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using mParticle.LoadGenerator.Models;
using Newtonsoft.Json;

namespace mParticle.LoadGenerator.Services
{
    public class ApiClient
    {
        private readonly Config config;

        private readonly HttpClient client;

        private readonly string postPath;

        public ApiClient(Config config, HttpClient client)
        {
            this.config = config;
            this.client = client;

            var serverUri = new Uri(this.config.ServerURL);
            this.client.BaseAddress = new Uri($"{serverUri.Scheme}://{serverUri.Host}");
            this.client.DefaultRequestHeaders.Add("X-Api-Key", this.config.AuthKey);
            this.postPath = serverUri.AbsolutePath;
        }

        public async Task<MyRequestResponse> CallApiEndpointAsync()
        {
            var myRequest = new MyRequest { Name = this.config.UserName };

            var requestBody = new StringContent(JsonConvert.SerializeObject(myRequest));
            
            try
            {
                //var response = await this.client.PostAsync(this.postPath, requestBody);
                //response.EnsureSuccessStatusCode();
                //return JsonConvert.DeserializeObject<MyRequestResponse>(await response.Content.ReadAsStringAsync());
                return await Task.FromResult(new MyRequestResponse() { Successful = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
