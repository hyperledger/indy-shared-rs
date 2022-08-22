using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class MasterSecretBlindingData
    {
        [JsonProperty("v_prime")]
        public string VPrime { get; set; }
        [JsonProperty("vr_prime")]
        public string VrPrime { get; set; }
    }
}