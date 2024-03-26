using Newtonsoft.Json;
using System;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryDefinition
    {
        public IntPtr Handle { get; set; }

        public string JsonString { get; set; } 

        [JsonProperty("ver")]
        public string Ver { get; set; }

        [JsonProperty("id")]
        public string RevocationRegistryId { get; set; }

        [JsonProperty("credDefId")]
        public string CredentialDefinitionId { get; set; }

        [JsonProperty("revocDefType")]
        public string RegistryType { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("value")]
        public RevocationRegistryDefinitionValue Value { get; set; }
    }
}