using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace BiblioMit.Models.VM
{
    public static class CSPTag
    {
        public static void Start()
        {
            BaseUri = new HashSet<string>
            {
                "'self'"
            };
            BlockAllMixedContent = true;
            DefaultSrc = new HashSet<string>
            {
                "'self'"
            };
            ConnectSrc = new HashSet<string>
            {
                "'self'",
                "https://fonts.googleapis.com/", "https://fonts.gstatic.com/",
                "https://www.facebook.com/", "https://web.facebook.com/",
                "https://dc.services.visualstudio.com/",
                "https://www.google-analytics.com/"
            };
            FrameSrc = new HashSet<string>
            {
                "'self'",
                "https://www.facebook.com/", "https://web.facebook.com/"
            };
            ImgSrc = new HashSet<string>
            {
                "data:","blob:","'self'"
            };
            ObjectSrc = new HashSet<string>
            {
                "'none'"
            };
            ScriptSrc = new HashSet<string>
            {
                "'self'"
            };
            ScriptSrcElem = new HashSet<string>
            {
                "'self'",
                "https://connect.facebook.net/","https://fonts.googleapis.com/",
                "'sha256-+T05iRYOHWysv8XMYTvF4/XxwtEF4mmqogAmskL2uwY='"
            };
            StyleSrc = new HashSet<string>
            {
                "'self'", "'unsafe-inline'"
            };
            StyleSrcElem = new HashSet<string>
            {
                "'self'","https://fonts.googleapis.com/", "'unsafe-inline'"
            };
            FontSrc = new HashSet<string>
            {
                "'self'","data:","https://fonts.gstatic.com/","https://fonts.googleapis.com/"
            };
            UpgradeInsecureRequests = true;
            AccessControlUrls = new HashSet<string>
            {
                "https://www.facebook.com",
                "https://web.facebook.com"
            };
        }
        public static HashSet<string> BaseUri { get; internal set; } = null!;
        public static bool BlockAllMixedContent { get; set; } = true;
        public static HashSet<string> DefaultSrc { get; internal set; } = null!;
        public static HashSet<string> ConnectSrc { get; internal set; } = null!;
        public static HashSet<string> FrameSrc { get; internal set; } = null!;
        public static HashSet<string> ImgSrc { get; internal set; } = null!;
        public static HashSet<string> ObjectSrc { get; internal set; } = null!;
        public static HashSet<string> ScriptSrc { get; internal set; } = null!;
        public static HashSet<string> ScriptSrcElem { get; internal set; } = null!;
        public static HashSet<string> StyleSrc { get; internal set; } = null!;
        public static HashSet<string> StyleSrcElem { get; internal set; } = null!;
        public static HashSet<string> FontSrc { get; internal set; } = null!;
        public static bool UpgradeInsecureRequests { get; set; } = true;
        public static HashSet<string> AccessControlUrls { get; internal set; } = null!;
        public static string GetString(HttpRequest request)
        {
            string baseUrl = request.Host.Host;
            if (baseUrl == "localhost")
            {
                baseUrl += ":*";
                ScriptSrcElem.Add($"https://{baseUrl}");
                ConnectSrc.Add($"https://{baseUrl}");
            }
            StringValues userAgent = request.Headers[HeaderNames.UserAgent];
            Parser uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);
            char separator = ' ';
            //string noport = baseUrl.Value.Split(":")[0];
            StyleSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
            StyleSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));

            ConnectSrc.Add($"ws://{baseUrl}");
            ConnectSrc.Add($"wss://{baseUrl}");

            string scriptSrc = string.Empty;
            string styleSrc = string.Empty;
            if (c.OS.Family == "iOS" || c.UA.Family == "Firefox")
            {
                ScriptSrc.UnionWith(ScriptSrcElem);
                StyleSrc.UnionWith(StyleSrcElem);
                if (ScriptSrc.Contains("'unsafe-inline'"))
                {
                    ScriptSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                scriptSrc = string.Join(separator, ScriptSrc.Prepend("script-src"));
                styleSrc = string.Join(separator, StyleSrc.Prepend("style-src"));
            }
            else
            {
                if (ScriptSrcElem.Contains("'unsafe-inline'"))
                {
                    ScriptSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                if (ScriptSrc.Contains("'unsafe-inline'"))
                {
                    ScriptSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                scriptSrc = string.Join($";{separator}",
                    ScriptSrc.Any() ? string.Join(separator, ScriptSrc.Prepend("script-src")) : null,
                ScriptSrcElem.Any() ? string.Join(separator, ScriptSrcElem.Prepend("script-src-elem")) : null);
                styleSrc = string.Join($";{separator}",
                     StyleSrc.Any() ? string.Join(separator, StyleSrc.Prepend("style-src")) : null,
                 StyleSrcElem.Any() ? string.Join(separator, StyleSrcElem.Prepend("style-src-elem")) : null);
            }

            List<string> csp = new();
            if (BaseUri.Any()) csp.Add(string.Join(separator, BaseUri.Prepend("base-uri")));
            if (BlockAllMixedContent) csp.Add("block-all-mixed-content");
            if (DefaultSrc.Any()) csp.Add(string.Join(separator, DefaultSrc.Prepend("default-src")));
            if (ConnectSrc.Any()) csp.Add(string.Join(separator, ConnectSrc.Prepend("connect-src")));
            if (FrameSrc.Any()) csp.Add(string.Join(separator, FrameSrc.Prepend("frame-src")));
            if (ImgSrc.Any()) csp.Add(string.Join(separator, ImgSrc.Prepend("img-src")));
            if (ObjectSrc.Any()) csp.Add(string.Join(separator, ObjectSrc.Prepend("object-src")));
            csp.Add(scriptSrc);
            csp.Add(styleSrc);
            if (FontSrc.Any()) csp.Add(string.Join(separator, FontSrc.Prepend("font-src")));
            if (UpgradeInsecureRequests) csp.Add("upgrade-insecure-requests");
            return string.Join($";{separator}", csp);
        }
        public static string GetAccessControlString() => string.Join(" ", AccessControlUrls);
    }
}
