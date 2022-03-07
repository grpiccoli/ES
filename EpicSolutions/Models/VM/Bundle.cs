using System.Collections.ObjectModel;

namespace BiblioMit.Models.VM
{
    public class BundleConfig
    {
        public string? OutputFileName { get; set; }
        public Collection<string>? InputFiles { get; set; }
    }
}
