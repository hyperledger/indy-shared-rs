using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace indy_shared_rs_dotnet.Models
{
    public class BlindingMsCorrectnessProof
    {
        public string C { get; set; }

        [JsonProperty("v_dash_cap")]
        public string VDashCap { get; set; }

        [JsonProperty("m_caps")]
        public MCaps MCaps { get; set; }

        [JsonProperty("r_caps")]
        public JObject RCaps { get; set; }
    }
}