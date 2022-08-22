using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class PrimaryPredicateInequalityProof
    {
        [JsonProperty("u")]
        public Dictionary<string, string> U { get; set; }

        [JsonProperty("r")]
        public Dictionary<string, string> R { get; set; }

        [JsonProperty("mj")]
        public string Mj { get; set; }

        [JsonProperty("alpha")]
        public string Alpha { get; set; }

        [JsonProperty("t")]
        public Dictionary<string, string> T { get; set; }

        [JsonProperty("predicate")]
        public Predicate Predicate { get; set; }
    }
}