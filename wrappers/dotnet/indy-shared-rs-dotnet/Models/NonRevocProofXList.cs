using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class NonRevocProofXList
    {
        [JsonProperty("rho")]
        public string Rho { get; set; }

        [JsonProperty("r")]
        public string R { get; set; }

        [JsonProperty("r_prime")]
        public string RPrime { get; set; }

        [JsonProperty("r_prime_prime")]
        public string RPrimePrime { get; set; }

        [JsonProperty("r_prime_prime_prime")]
        public string RPrimePrimePrime { get; set; }

        [JsonProperty("o")]
        public string O { get; set; }

        [JsonProperty("o_prime")]
        public string OPrime { get; set; }

        [JsonProperty("m")]
        public string M { get; set; }

        [JsonProperty("m_prime")]
        public string MPrime { get; set; }

        [JsonProperty("t")]
        public string T { get; set; }

        [JsonProperty("t_prime")]
        public string TPrime { get; set; }

        [JsonProperty("m2")]
        public string M2 { get; set; }

        [JsonProperty("s")]
        public string S { get; set; }

        [JsonProperty("c")]
        public string C { get; set; }
    }
}