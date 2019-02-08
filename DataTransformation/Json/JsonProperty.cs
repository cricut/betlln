namespace Betlln.Data.Integration.Json
{
    public class JsonProperty
    {
        public JsonProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; set; }
    }
}