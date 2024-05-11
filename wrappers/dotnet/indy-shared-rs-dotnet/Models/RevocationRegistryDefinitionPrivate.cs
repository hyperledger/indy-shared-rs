using Newtonsoft.Json;
using System;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryDefinitionPrivate
    {
        public IntPtr Handle { get; set; }

        public string JsonString { get; set; }

        [JsonProperty("value")]
        public RevocationRegistryDefinitionPrivateValue Value { get; set; }
    }
}