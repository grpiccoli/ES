using BiblioMit.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BiblioMit.Pwa
{
    internal class WebManifestCache
    {
        private readonly IWebHostEnvironment _env;
        private readonly MemoryCache _cache;
        private readonly string _fileName;

        public WebManifestCache(IWebHostEnvironment env, string fileName)
        {
            _env = env;
            _fileName = fileName;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public WebManifest GetManifest()
        {
            return _cache.GetOrCreate("webmanifest", (entry) =>
            {
                IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_fileName);
                entry.AddExpirationToken(_env.WebRootFileProvider.Watch(_fileName));

                string json = File.ReadAllText(file.PhysicalPath);

                WebManifest? manifest = JsonSerializer.Deserialize<WebManifest>(json, JsonCase.Snake);
                if (manifest is null)
                {
                    throw new JsonException("manifest is null");
                }

                if (!manifest.IsValid(out string error))
                {
                    throw new JsonException(error);
                }
                manifest.FileName = _fileName;
                manifest.RawJson = Regex.Replace(json, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

                return manifest;
            });
        }
    }
}
