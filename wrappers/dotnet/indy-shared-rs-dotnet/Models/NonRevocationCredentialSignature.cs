using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class NonRevocationCredentialSignature
    {
        [JsonProperty("sigma")]
        public string Sigma { get; set; }

        [JsonProperty("c")]
        public string C { get; set; }

        [JsonProperty("vr_prime_prime")]
        public string VrPrimePrime { get; set; }

        [JsonProperty("witness_signature")]
        public WitnessSignature WitnessSignature { get; set; }

        [JsonProperty("g_i")]
        public string Gi { get; set; }

        [JsonProperty("i")]
        public uint I { get; set; }

        [JsonProperty("m2")]
        public string M2 { get; set; }
    }
}