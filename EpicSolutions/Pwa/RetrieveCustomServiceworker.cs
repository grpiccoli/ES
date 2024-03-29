﻿using Microsoft.Extensions.FileProviders;

namespace BiblioMit.Pwa
{
    /// <summary>
    /// A utility that can retrieve the contents of a CustomServiceworker strategy file
    /// </summary>
    public class RetrieveCustomServiceworker
    {
        private readonly IWebHostEnvironment _env;

        public RetrieveCustomServiceworker(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Returns a <seealso cref="string"/> containing the contents of a Custom Serviceworker javascript file
        /// </summary>
        /// <returns></returns>
        public string GetCustomServiceworker(string fileName = "customserviceworker.js")
        {
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(fileName);
            return File.ReadAllText(file.PhysicalPath);
        }
    }
}
