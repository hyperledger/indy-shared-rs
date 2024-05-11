using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class Presentation
    {
        [JsonProperty("proof")]
        public PresentationProof Proof { get; set; }

        [JsonProperty("requested_proof")]
        public RequestedProof RequestedProof { get; set; }

        [JsonProperty("identifiers")]
        public List<Identifier> Identifiers { get; set; }

        public IntPtr Handle { get; set; }
        public string JsonString { get; set; }
    }
}