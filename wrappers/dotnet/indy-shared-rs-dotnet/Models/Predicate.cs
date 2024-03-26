using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class Predicate
    {
        [JsonProperty("attr_name")]
        public string AttrName { get; set; }

        [JsonProperty("p_type")]
        public PredicateTypes PredicateType { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}