using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class PresentationProof
    {
        [JsonProperty("proofs")]
        public List<SubProof> Proofs { get; set; }
        [JsonProperty("aggregated_proof")]
        public AggregatedProof AggregatedProof { get; set; }
    }
}