namespace Dbank.Digisoft.Consul.Models
{
    public class JsonInputModel
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string Path { get; set; } = null!;
        public override string ToString() => $"{Key}={Value}";
    }
}
