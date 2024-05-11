using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class AggregatedProof
    {
        [JsonProperty("c_hash")]
        public string CHash { get; set; }

        [JsonProperty("c_list")]
        public List<List<byte>> CList { get; set; }
    }
}