using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace BiblioMit.Controllers
{
    [Route("/")]
    [ApiController]
    public class SitemapController : ControllerBase
    {
        private static readonly List<Uri> lists = new() { new Uri("https://www.bibliomit.cl") };
        [Route("/sitemap.xml")]
        public async Task Invoke()
        {
            Stream stream = HttpContext.Response.Body;
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = "application/xml";
            string sitemapContent = "<urlset xmlns=\"https://www.sitemaps.org/schemas/sitemap/0.9\">";
            IEnumerable<Type> controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type)
                || type.Name.EndsWith("controller", StringComparison.CurrentCultureIgnoreCase));

            foreach (Type controller in controllers)
            {
                int cnt = 0;
                MethodInfo[] methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods)
                {
                    bool test1 = method.ReturnType.Name == "ActionResult"
                        || method.ReturnType.Name == "IActionResult" || method.ReturnType.Name == "Task`1";
                    //only for websites with intranet
                    //var test2 = method.CustomAttributes.Any(c => c.AttributeType == typeof(AllowAnonymousAttribute));

                    //if(test1 && test2)
                    if (test1)
                    {
                        cnt++;
                        foreach (Uri uri in lists)
                        {
                            sitemapContent += "<url>";
                            sitemapContent += string.Format(CultureInfo.InvariantCulture,
                                "<loc>{0}/{1}/{2}</loc>", uri,
                            controller.Name.ToUpperInvariant()
                            .Replace("controller", "", StringComparison.CurrentCultureIgnoreCase),
                            method.Name.ToUpperInvariant());
                            sitemapContent += string.Format(CultureInfo.InvariantCulture,
                                "<lastmod>{0}</lastmod>", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                            sitemapContent += "</url>";
                        }
                    }
                }
            }
            sitemapContent += "</urlset>";
            using MemoryStream memoryStream = new();
            byte[] bytes = Encoding.UTF8.GetBytes(sitemapContent);
            await memoryStream.WriteAsync(bytes.AsMemory(0, bytes.Length)).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(stream, bytes.Length).ConfigureAwait(false);
        }
    }
}
