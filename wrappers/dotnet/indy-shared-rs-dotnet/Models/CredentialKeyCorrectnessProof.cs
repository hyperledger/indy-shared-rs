using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class CredentialKeyCorrectnessProof
    {
        [JsonIgnore]
        public IntPtr Handle { get; set; }
        public string JsonString { get; set; }
        public string C { get; set; }

        [JsonProperty("xz_cap")]
        public string XzCap { get; set; }

        [JsonIgnore]
        public List<KeyProofAttributeValue> XrCap { get; set; }
    }
}