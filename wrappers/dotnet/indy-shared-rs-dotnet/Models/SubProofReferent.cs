using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class SubProofReferent
    {
        [JsonProperty("sub_proof_index")]
        public uint SubProofIndex { get; set; }
    }
}