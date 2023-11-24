namespace Dbank.Digisoft.Config.Web.Models
{
    public class IndexViewModel
    {
        public bool IsFile { get; set; }
        public string Output { get; set; } = string.Empty;
        public List<(string, string)> ViewList { get; set; } = new();
        public string Directory { get; set; } = string.Empty;
    }
}
