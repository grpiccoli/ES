using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AspNetCore.IServiceCollection.AddIUrlHelper;
using EpicSolutions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;

namespace EpicSolutions
{
    public class Startup
    {
        private readonly string _os = Environment.OSVersion.Platform.ToString();
        //private readonly string _corsOrigins = "_safeOrigins";
        private const string defaultCulture = "en";
        private readonly CultureInfo[] supportedCultures;
        public Startup(IConfiguration configuration)
        {
            supportedCultures = new[]
                {
                    new CultureInfo(defaultCulture),
                    new CultureInfo("es")
                };
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddNewtonsoftJson();

            services.AddCors();

            //services.AddCors(options => 
            //    options.AddPolicy(_corsOrigins,
            //        builder => 
            //        builder.WithOrigins("https://www.facebook.com")
            //        ));

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
                //options.ExcludedHosts.Add("https://www.facebook.com");
            });

            services.AddHttpsRedirection(options =>
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                // Formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                options.SupportedUICultures = supportedCultures;
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddUrlHelper();

            Libman.LoadJson();
            Bundler.LoadJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null || env == null) return;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseCors();
            //app.UseCors(_corsOrigins);
            //app.Use(async (ctx, next) =>
            //{
            //    ctx.Response.Headers.Add("Context-Security-Policy", "default-src 'self';script-src 'self' *.facebook.com *.googletagmanager.com tagmanager.google.com *.google-analytics.com *.cloudflare.com disqus.com *.disqus.com *.disquscdn.com cdn.ampproject.org cdn.jsdelivr.net oss.maxcdn.com 'nonce-I3qrCKfNVOOdKakCh6DLehJ97ZHZHepQ1b4yZu6EUHs=' 'unsafe-eval';style-src 'self' *.googleapis.com *.googleusercontent.com *.googletagmanager.com tagmanager.google.com *.cloudflare.com *.disquscdn.com *.buttercms.com 'unsafe-inline';connect-src 'self' *.disqus.com *.googletagmanager.com tagmanager.google.com data:;font-src 'self' *.googleapis.com *.gstatic.com data:;img-src 'self' data: *.buttercms.com *.googleusercontent.com *.google-analytics.com *.googletagmanager.com *.gstatic.com *.disquscdn.com *.disqus.com tagmanager.google.com;media-src 'none';object-src 'none';frame-ancestors 'none';frame-src *.googletagmanager.com tagmanager.google.com disqus.com *.facebook.com");
            //    await next().ConfigureAwait(false);
            //});
            app.UseRouting();
            app.UseDefaultImage(defaultImagePath: Configuration.GetSection($"{_os}defaultImagePath").Value);
            var path = new List<string> { "wwwroot", "lib", "cldr-data", "main" };
            var ch = _os == "Win32NT" ? @"\" : "/";
            var di = new DirectoryInfo(Path.Combine(env.ContentRootPath, string.Join(ch, path)));
            var supportedCultures = di.GetDirectories().Where(x => x.Name != "root").Select(x => new CultureInfo(x.Name)).ToList();
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(supportedCultures.FirstOrDefault(x => x.Name == "es")),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
            });
            app.UseCookiePolicy();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
