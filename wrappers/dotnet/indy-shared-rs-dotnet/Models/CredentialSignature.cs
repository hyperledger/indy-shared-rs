using Newtonsoft.Json;

namespace indy_shared_rs_dotnet.Models
{
    public class CredentialSignature
    {
        [JsonProperty("p_credential")]
        public PrimaryCredentialSignature PCredential { get; set; }
        [JsonProperty("r_credential")]
        public NonRevocationCredentialSignature RCredential { get; set; }
    }
}