using System;
using System.Collections.Generic;

namespace EpicSolutions.Models.VM
{
    public class Libs
    {
        public string DefaultProvider { get; set; }
        public List<LibManLibrary> Libraries { get; } = new List<LibManLibrary>();
    }
}
