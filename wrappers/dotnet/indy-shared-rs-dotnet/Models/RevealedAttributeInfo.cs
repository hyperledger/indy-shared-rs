using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class RevealedAttributeInfo
    {
        [JsonProperty("sub_proof_index")]
        public uint SubProofIndex { get; set; }
        public string Raw { get; set; }
        public string Encoded { get; set; }
    }
}