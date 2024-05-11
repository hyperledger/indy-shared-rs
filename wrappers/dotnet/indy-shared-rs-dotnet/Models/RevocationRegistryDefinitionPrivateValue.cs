using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryDefinitionPrivateValue
    {
        [JsonProperty("gamma")]
        public string Gamma { get; set; }
    }
}