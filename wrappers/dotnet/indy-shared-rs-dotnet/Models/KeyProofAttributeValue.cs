namespace indy_shared_rs_dotnet.Models
{
    public class KeyProofAttributeValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public KeyProofAttributeValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}