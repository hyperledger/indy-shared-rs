using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class PrimaryCredentialSignature
    {
        [JsonProperty("m_2")]
        public string M2 { get; set; }

        [JsonProperty("a")]
        public string A { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("v")]
        public string V { get; set; }
    }
}