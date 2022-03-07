using BiblioMit.Extensions;
using BiblioMit.Models.VM;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BiblioMit.Services
{
    public class LibHashModel
    {
        public string? Key { get; set; }
        public List<SourcesModel>? Values { get; set; }
    }
    public class SourcesModel
    {
        public SourcesModel() { }
        public SourcesModel SetLibMan(LibManLibrary l, string file)
        {
            if (l.Provider == "cdnjs") l.Library = l.Library.Replace("@", "/");
            WgetArgs = $"{Href} -O {l.Destination}/{file}";
            Fallback = $"{l.Destination}/{file}";
            //https required for this
            Extension = Path.GetExtension(file).TrimStart('.');
            if (Href is not null)
                Hash = Extensions.Hash.Get512Async(new Uri(Href)).Result;
            Preload = !l.Library.StartsWith("nanogallery2");
            LibType = Extension switch
            {
                "js" => LibType.jsRemote,
                "css" => LibType.cssRemote,
                _ => Preload ? LibType.fontRemotePreload : LibType.fontRemote
            };
            return this;
        }
        public SourcesModel(LibManLibrary library, string file)
        {
            string url = library.Provider switch
            {
                "cdnjs" => "cdnjs.cloudflare.com/ajax/libs",
                "unpkg" => "unpkg.com",
                _ => "cdn.jsdelivr.net/npm"
            };
            Href = $"https://{url}/{library.Library}/{file}";
        }
        public SourcesModel(BundleConfig bundle)
        {
            Href = bundle.OutputFileName?.Replace("wwwroot", "~") ?? string.Empty;
            Extension = Path.GetExtension(Href).TrimStart('.');
            LibType = Extension switch
            {
                "js" => LibType.jsLocal,
                "css" => LibType.cssLocal,
                _ => LibType.fontLocal
            };
        }
        public SourcesModel(Compile compile)
        {
            Href = compile.OutputFile?.Replace("wwwroot", "~") ?? string.Empty;
            Extension = Path.GetExtension(Href).TrimStart('.');
            LibType = LibType.cssLocal;
        }
        public string? WgetArgs { get; set; }
        public string Href { get; set; } = null!;
        public string? Hash { get; set; }
        public string? Fallback { get; set; }
        public string? Extension { get; set; }
        public bool Preload { get; set; } = true;
        public LibType LibType { get; set; }
    }
    public enum LibType
    {
        cssLocal,
        cssRemote,
        jsLocal,
        jsRemote,
        fontLocal,
        fontRemote,
        fontRemotePreload
    }
    public static class Libman
    {
        private const string _latestVersion = "1.0";
        private const string _filename = "libhash.json";
        private const string _libman = "libman.json";
        private const string _bundler = "bundleconfig.json";
        private const string _compiler = "compilerconfig.json";
        private static SortedDictionary<string, HashSet<SourcesModel>> Libs { get; set; } = new();
        public static void LoadJson()
        {
            string filePath = Path.Combine("StaticFiles", "Json", _filename);
            using StreamReader r = new(filePath);
            string json = r.ReadToEnd();
            SortedDictionary<string, HashSet<SourcesModel>>? libhashes = JsonSerializer.Deserialize<SortedDictionary<string, HashSet<SourcesModel>>>(json);
            if (libhashes == null)
            {
                throw new FileLoadException($"Unable to read file {_libman}");
            }
            r.Close();
            Libs = libhashes;

            using StreamReader l = new(_libman);
            json = l.ReadToEnd();
            Libs? libs = JsonSerializer.Deserialize<Libs>(json, JsonCase.Camel);
            if (libs == null || libs.Libraries == null)
            {
                throw new FileLoadException($"Unable to read file {_libman}");
            }

            foreach (LibManLibrary library in libs.Libraries)
            {
                string key = Regex.Replace(library.Library, @"@[^@]+$", "");
                IEnumerable<string> files = library.Files
                        .Where(f => !f.EndsWith("gif") && !f.EndsWith("png"));
                if (Libs.ContainsKey(key))
                {
                    if (files.Count() != Libs[key].Count)
                    {
                        Libs.Remove(key);
                        Libs.Add(key, new HashSet<SourcesModel>(files.Select(f => new SourcesModel(library, f).SetLibMan(library, f))));
                    }
                    else
                    {
                        foreach (string file in files)
                        {
                            SourcesModel model = new(library, file);
                            if (!Libs[key].Any(x => x.Href == model.Href))
                            {
                                model.SetLibMan(library, file);
                                Libs[key].Add(model);
                            }
                        }
                    }
                }
                else
                {
                    Libs.Add(key, new HashSet<SourcesModel>(files.Select(f => new SourcesModel(library, f).SetLibMan(library, f))));
                }
            }
            PlatformID os = Environment.OSVersion.Platform;
            if (PlatformID.Win32NT != os && libs.Version == _latestVersion)
            {
                foreach (HashSet<SourcesModel> lib in Libs.Values)
                {
                    foreach (SourcesModel file in lib)
                    {
                        using Process process = new()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "wget",
                                Arguments = file.WgetArgs,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            }
                        };
                        string s = string.Empty;
                        string e = string.Empty;
                        process.OutputDataReceived += (sender, data) => s += data.Data;
                        process.ErrorDataReceived += (sender, data) => e += data.Data;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        process.Close();
                    }
                }
            }
            //bundler
            using StreamReader b = new(_bundler);
            json = b.ReadToEnd();
            IEnumerable<BundleConfig>? bundles = JsonSerializer.Deserialize<IEnumerable<BundleConfig>>(json, JsonCase.Camel);
            if (bundles != null)
            {
                foreach (BundleConfig bundle in bundles)
                {
                    string key = Regex.Replace(bundle.OutputFileName ?? string.Empty, @"^wwwroot/.*/(.*)(.min)?.(css|js|woff2|woff|ttf)$", "$1").Replace(".min", "");
                    if (!Libs.ContainsKey(key))
                    {
                        Libs[key] = new HashSet<SourcesModel>
                        {
                            new SourcesModel(bundle)
                        };
                    }
                    else
                    {
                        SourcesModel model = new(bundle);
                        if (!Libs[key].Any(l => l.Href == model.Href))
                        {
                            Libs[key].Add(model);
                        }
                    }
                }
            }
            using StreamReader c = new(_compiler);
            string jsonC = c.ReadToEnd();
            IEnumerable<Compile>? compiles = JsonSerializer.Deserialize<IEnumerable<Compile>>(jsonC, JsonCase.Camel);
            if (compiles != null)
            {
                foreach (Compile compile in compiles)
                {
                    string key = Regex.Replace(compile.OutputFile ?? string.Empty, @"^wwwroot/.*/(.*).css$", "$1")
                    .Replace(".css", ".min.css");
                    if (!Libs.ContainsKey(key))
                    {
                        Libs[key] = new HashSet<SourcesModel>
                        {
                            new SourcesModel(compile)
                        };
                    }
                    else
                    {
                        SourcesModel model = new(compile);
                        if (!Libs[key].Any(l => l.Href == model.Href))
                        {
                            Libs[key].Add(model);
                        }
                    }
                }
            }
            using StreamWriter w = new(filePath);
            w.Write(JsonSerializer.Serialize(Libs));
        }
#if DEBUG
        public static HashSet<SourcesModel> GetLibs(string lib) =>
            Libs[lib];
#else
        public static HashSet<SourcesModel>? GetLibs(string lib) =>
            Libs.ContainsKey(lib) ? Libs[lib] : null;
#endif
    }
}
