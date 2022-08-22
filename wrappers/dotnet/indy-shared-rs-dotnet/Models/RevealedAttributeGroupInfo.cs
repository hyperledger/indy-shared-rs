using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class RevealedAttributeGroupInfo
    {
        [JsonProperty("sub_proof_index")]
        public uint SubProofIndex { get; set; }
        public Dictionary<string, AttributeValue> Values { get; set; }
    }
}