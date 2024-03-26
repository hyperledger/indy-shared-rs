using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class Witness
    {
        [JsonProperty("omega")]
        public string Omega { get; set; }
    }
}