using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class Credential
    {
        [JsonProperty("schema_id")]
        public string SchemaId { get; set; }

        [JsonProperty("cred_def_id")]
        public string CredentialDefinitionId { get; set; }

        [JsonProperty("rev_reg_id")]
        public string RevocationRegistryId { get; set; } = null;
        public Dictionary<string, AttributeValue> Values { get; set; }

        [JsonProperty("signature")]
        public CredentialSignature Signature { get; set; }

        [JsonProperty("signature_correctness_proof")]
        public SignatureCorrectnessProofValue SignatureCorrectnessProof { get; set; }

        [JsonProperty("rev_reg")]
        public RevocationRegistryValue RevocationRegistry { get; set; } = null;

        [JsonProperty("witness")]
        public Witness Witness { get; set; } = null;
        public string JsonString { get; set; }
        public IntPtr Handle { get; set; }
    }
}