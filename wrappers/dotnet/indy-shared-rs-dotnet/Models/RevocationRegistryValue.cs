using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryValue
    {
        [JsonProperty("accum")]
        public string Accum { get; set; }
    }
}