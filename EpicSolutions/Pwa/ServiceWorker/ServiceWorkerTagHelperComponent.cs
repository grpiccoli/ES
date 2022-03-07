using BiblioMit.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BiblioMit.Pwa
{
    internal class ServiceWorkerTagHelperComponent : TagHelperComponent
    {
        private readonly string _script;

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;
        private readonly PwaOptions _options;

        public ServiceWorkerTagHelperComponent(IWebHostEnvironment env, IHttpContextAccessor accessor, PwaOptions options)
        {
            _env = env;
            _accessor = accessor;
            _options = options;
            string csp = string.Empty;
            if (_options.EnableCspNonce)
            {
                string serviceWorkerNonce = Hash.Nonce();
                Models.VM.CSPTag.ScriptSrcElem.Add($"'nonce-{serviceWorkerNonce}'");
                csp = string.Format(Constants.CspNonce, serviceWorkerNonce);
            }
            _script = $"\r\n\t<script{csp}>'serviceWorker' in navigator&&navigator.serviceWorker.register('{options.BaseRoute}{Constants.ServiceworkerRoute}', {{ scope: '{options.BaseRoute}/' }})</script>";
        }

        /// <inheritdoc />
        public override int Order => -1;

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!_options.RegisterServiceWorker)
            {
                return;
            }

            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                if ((_options.AllowHttp || (_accessor.HttpContext is not null && _accessor.HttpContext.Request.IsHttps)) || _env.IsDevelopment())
                {
                    output.PostContent.AppendHtml(_script);
                }
            }
        }
    }
}
