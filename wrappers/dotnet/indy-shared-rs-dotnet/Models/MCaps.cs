using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class MCaps
    {
        [JsonProperty("master_secret")]
        public string MasterSecret { get; set; }
    }
}