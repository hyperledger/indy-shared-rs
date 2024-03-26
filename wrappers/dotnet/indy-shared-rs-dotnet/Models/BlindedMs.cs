using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class BlindedMs
    {
        public string U { get; set; }

        public string Ur { get; set; }

        [JsonProperty("hidden_attributes")]
        public List<string> HiddenAttributes { get; set; }

        [JsonProperty("committed_attributes")]
        public JObject ComittedAttributes { get; set; }
    }
}