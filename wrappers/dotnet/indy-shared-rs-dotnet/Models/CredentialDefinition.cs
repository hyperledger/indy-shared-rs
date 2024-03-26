using Newtonsoft.Json;
using System;

namespace indy_shared_rs_dotnet.Models
{
    public class CredentialDefinition
    {
        public IntPtr Handle { get; set; }
        public string JsonString { get; set; }

        [JsonProperty("id")]
        public string CredentialDefinitionId { get; set; }

        [JsonProperty("schemaId")]
        public string SchemaId { get; set; }

        [JsonProperty("type")]
        public string SignatureType { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("ver")]
        public string Ver { get; set; }

        public CredentialDefinitionData Value { get; set; }
    }
}