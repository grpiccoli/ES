namespace BiblioMit.Models.VM
{
    public class LibManLibrary
    {
        public string Library { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new List<string>();
        public string? Provider { get; set; }
    }
}
