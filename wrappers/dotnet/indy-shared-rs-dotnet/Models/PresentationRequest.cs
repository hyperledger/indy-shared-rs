using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class PresentationRequest
    {
        public string Nonce { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string JsonString { get; set; }
        public Dictionary<string, AttributeInfo> RequestedAttributes { get; set; }
        public Dictionary<string, PredicateInfo> RequestedPredicates { get; set; }
        public IntPtr Handle { get; set; }
    }
}