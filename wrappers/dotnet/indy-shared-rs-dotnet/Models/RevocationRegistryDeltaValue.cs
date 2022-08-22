using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryDeltaValue
    {
        [JsonProperty("prevAccum")]
        public string PrevAccum { get; set; }

        [JsonProperty("accum")]
        public string Accum { get; set; }

        [JsonProperty("issued")]
        public HashSet<uint> Issued { get; set; }

        [JsonProperty("revoked")]
        public HashSet<uint> Revoked { get; set; }
    }
}