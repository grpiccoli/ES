namespace BiblioMit.Models.VM
{
    public class Libs
    {
        public string? Version { get; set; }
        public string? DefaultProvider { get; set; }
        public IEnumerable<LibManLibrary>? Libraries { get; set; }
    }
}
