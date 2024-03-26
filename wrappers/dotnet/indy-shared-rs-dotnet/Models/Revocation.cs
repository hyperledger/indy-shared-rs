using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class Revocation
    {
        public string G { get; set; }

        [JsonProperty("g_dash")]
        public string GDash { get; set; }
        public string H { get; set; }
        public string H0 { get; set; }
        public string H1 { get; set; }
        public string H2 { get; set; }
        public string HTilde { get; set; }

        [JsonProperty("h_cap")]
        public string HCap { get; set; }
        public string U { get; set; }
        public string Pk { get; set; }
        public string Y { get; set; }
    }
}