using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryDefinitionValuePublicKeys
    {
        [JsonProperty("accumKey")]
        public AccumKey AccumKey { get; set; }
    }
}