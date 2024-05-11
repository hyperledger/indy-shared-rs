using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class RequestedProof
    {
        [JsonProperty("revealed_attrs")]
        public Dictionary<string, RevealedAttributeInfo> RevealedAttrs { get; set; }

        [JsonProperty("revealed_attrs_groups")]
        public Dictionary<string, RevealedAttributeGroupInfo> RevealedAttrGroups { get; set; }

        [JsonProperty("self_attested_attrs")]
        public Dictionary<string, string> SelfAttestedAttrs { get; set; }

        [JsonProperty("unrevealed_attrs")]
        public Dictionary<string, SubProofReferent> UnrevealedAttrs { get; set; }

        [JsonProperty("predicates")]
        public Dictionary<string, SubProofReferent> Predicates { get; set; }
    }
}