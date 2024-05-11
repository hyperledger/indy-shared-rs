using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class PrimaryEqualProof
    {
        [JsonProperty("revealed_attrs")]
        public Dictionary<string, string> RevealedAttrs { get; set; }

        [JsonProperty("a_prime")]
        public string APrime { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("v")]
        public string V { get; set; }

        [JsonProperty("m")]
        public Dictionary<string, string> M { get; set; }

        [JsonProperty("m2")]
        public string M2 { get; set; }
    }
}