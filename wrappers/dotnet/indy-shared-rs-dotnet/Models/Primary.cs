using Newtonsoft.Json;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class Primary
    {
        public string N { get; set; }
        public string S { get; set; }

        [JsonIgnore]
        public List<KeyProofAttributeValue> R { get; set; }
        public string Rctxt { get; set; }
        public string Z { get; set; }
    }
}