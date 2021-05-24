using System;
using System.Text.Json.Serialization;

namespace mParticle.LoadGenerator.Models
{
    public class MyRequest
    {
        public string Name { get; set; }
        
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("requests_sent")]
        public uint RequestsSent { get; set; }
    }
}
