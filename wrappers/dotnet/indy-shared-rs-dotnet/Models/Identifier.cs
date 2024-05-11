using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class Identifier
    {
        [JsonProperty("schema_id")]
        public string SchemaId { get; set; }

        [JsonProperty("cred_def_id")]
        public string CredentialDefinitionId { get; set; }

        [JsonProperty("rev_reg_id")]
        public string RevocationRegistryId { get; set; }

        [JsonProperty("timestamp")]
        public uint Timestamp { get; set; }
    }
}