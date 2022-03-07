using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;

namespace EpicSolutions.Extensions
{
    [HtmlTargetElement("source", Attributes = AppendVersionAttributeName + "," + SrcAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class SourceTagHelper : UrlResolutionTagHelper
    {
        private const string AppendVersionAttributeName = "asp-append-version";
        private const string SrcAttributeName = "srcset";

        /// <summary>
        /// Creates a new <see cref="ImageTagHelper"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/> to use.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version.")]
        public SourceTagHelper(
            IWebHostEnvironment hostingEnvironment,
            IMemoryCache cache,
            HtmlEncoder htmlEncoder,
            IUrlHelperFactory urlHelperFactory)
            : base(urlHelperFactory, htmlEncoder)
        {
            HostingEnvironment = hostingEnvironment;
            Cache = cache;
        }

        /// <summary>
        /// Creates a new <see cref="ImageTagHelper"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="cacheProvider">The <see cref="TagHelperMemoryCacheProvider"/>.</param>
        /// <param name="fileVersionProvider">The <see cref="IFileVersionProvider"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/> to use.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        // Decorated with ActivatorUtilitiesConstructor since we want to influence tag helper activation
        // to use this constructor in the default case.
        [ActivatorUtilitiesConstructor]
        public SourceTagHelper(
            IWebHostEnvironment hostingEnvironment,
            TagHelperMemoryCacheProvider cacheProvider,
            IFileVersionProvider fileVersionProvider,
            HtmlEncoder htmlEncoder,
            IUrlHelperFactory urlHelperFactory)
            : base(urlHelperFactory, htmlEncoder)
        {
            HostingEnvironment = hostingEnvironment;
            Cache = cacheProvider?.Cache;
            FileVersionProvider = fileVersionProvider;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// Source of the image.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(SrcAttributeName)]
        public string Src { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the src urls.
        /// </summary>
        /// <remarks>
        /// If <c>true</c> then a query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool AppendVersion { get; set; }

        protected internal IWebHostEnvironment HostingEnvironment { get; }

        protected internal IMemoryCache Cache { get; }

        internal IFileVersionProvider FileVersionProvider { get; private set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.CopyHtmlAttribute(SrcAttributeName, context);
            ProcessUrlAttribute(SrcAttributeName, output);

            if (AppendVersion)
            {
                EnsureFileVersionProvider();

                // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
                // pipeline have touched the value. If the value is already encoded this ImageTagHelper may
                // not function properly.
                Src = output.Attributes[SrcAttributeName].Value as string;

                output.Attributes.SetAttribute(SrcAttributeName, FileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, Src));
            }
        }

        private void EnsureFileVersionProvider()
        {
            if (FileVersionProvider == null)
            {
                FileVersionProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();
            }
        }
    }
}
