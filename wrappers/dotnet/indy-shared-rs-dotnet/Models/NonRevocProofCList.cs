using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class NonRevocProofCList
    {
        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("d")]
        public string D { get; set; }

        [JsonProperty("a")]
        public string A { get; set; }

        [JsonProperty("g")]
        public string G { get; set; }

        [JsonProperty("w")]
        public string W { get; set; }

        [JsonProperty("s")]
        public string S { get; set; }

        [JsonProperty("u")]
        public string U { get; set; }
    }
}