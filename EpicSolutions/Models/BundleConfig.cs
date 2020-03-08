using System.Collections.Generic;

namespace EpicSolutions.Models
{
    public class BundleConfig
    {
        public string OutputFileName { get; set; }
        public List<string> InputFiles { get; } = new List<string>();
    }
}
