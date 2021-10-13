using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ComX.Common.ApiClients.Tests.Models
{
    public class Person
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Surname")]
        public string Surname { get; set; }

        [JsonProperty("NumeleDat")]
        public string GivenName { get; set; }
    
        [JsonPropertyName("CuloareaMea")]
        public Colors Color { get; set; }
    }

}
