using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Text;
using System.Text.Encodings.Web;

namespace BiblioMit.Extensions
{
    public static class ViewPageExtensions
    {
        private const string BLOCK_BUILDER = "BlockBuilder";
        private static HashSet<Func<dynamic, HelperResult>> Libs { get; set; } = new HashSet<Func<dynamic, HelperResult>>();
        public static HtmlString Blocks(this RazorPageBase webPage, string name, params Func<dynamic, HelperResult>[] templates)
        {
            if (templates is null)
            {
                throw new ArgumentNullException(nameof(templates));
            }

            StringBuilder sb = new();
            foreach (Func<dynamic, HelperResult> t in templates)
            {
                sb.Append(webPage.Block(name, t));
            }
            return new HtmlString(sb.ToString());
        }
        public static HtmlString Block(this RazorPageBase webPage, string name, Func<dynamic, HelperResult> template)
        {
            if (Libs.Contains(template))
            {
                return new HtmlString(string.Empty);
            }

            StringBuilder sb = new();
            using TextWriter tw = new StringWriter(sb);
            HtmlEncoder? encoder = (HtmlEncoder?)webPage.ViewContext.HttpContext.RequestServices.GetService(typeof(HtmlEncoder));
            if (encoder == null)
            {
                throw new InvalidOperationException();
            }

            if (webPage.ViewContext.HttpContext.Request.Headers["x-requested-with"] != "XMLHttpRequest")
            {
                StringBuilder scriptBuilder = webPage.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] as StringBuilder ?? new();

                template.Invoke(null!).WriteTo(tw, encoder);
                scriptBuilder.Append(sb);

                webPage.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] = scriptBuilder;
                Libs.Add(template);
                return new HtmlString(string.Empty);
            }

            template.Invoke(null!).WriteTo(tw, encoder);

            return new HtmlString(sb.ToString());
        }
        public static HtmlString WriteBlocks(this RazorPageBase webPage, string name)
        {
            StringBuilder scriptBuilder = webPage?.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] as StringBuilder ?? new();

            return new HtmlString(scriptBuilder.ToString());
        }
    }
}
