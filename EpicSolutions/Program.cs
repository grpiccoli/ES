using BiblioMit.Models.VM;
using BiblioMit.Pwa;
using BiblioMit.Services;
using EpicSolutions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;

string os = Environment.OSVersion.Platform.ToString();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    config.AddJsonFile($"appsettings.{os}.json", optional: true, reloadOnChange: true));

builder.WebHost
    .UseKestrel(c =>
    {
        c.AddServerHeader = false;
        c.Limits.MaxConcurrentConnections = 200;
        c.Limits.MaxConcurrentUpgradedConnections = 200;
    });

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    //This setting is in csproj
    //options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    // Formatting numbers, dates, etc.
    options.SupportedCultures = Statics.SupportedCultures;
    // UI strings that we have localized.
    options.SupportedUICultures = Statics.SupportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddResponseCaching();

builder.Services.AddControllersWithViews(
//    options => {
// this requires all GETS to have antiforgery token
//    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
// this would only allow authenticated users by default
//    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
//}
    )
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    ;

builder.Services.AddHsts(
    options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
        options.ExcludedHosts.Add("bibliomit.cl");
        options.ExcludedHosts.Add("www.bibliomit.cl");
    }
);

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

builder.Services.AddProgressiveWebApp(new PwaOptions
{
    RegisterServiceWorker = true,
    RoutesToPreCache = "/Home/Index"
});

Libman.LoadJson();
CSPTag.Start();

builder.Services.AddScoped<IUrlHelper>(x =>
{
    ActionContext? actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
    if (actionContext == null) throw new NullReferenceException(nameof(actionContext));
    IUrlHelperFactory factory = x.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext);
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
}

app.UseReferrerPolicy(opts => opts.NoReferrer());

FileExtensionContentTypeProvider provider = new();
provider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseHttpsRedirection();

app.UseCors();

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        const int durationInSecond = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSecond;
    }
});

app.UseRouting();

app.UseResponseCaching();

IOptions<RequestLocalizationOptions>? localOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (localOptions != null) app.UseRequestLocalization(localOptions.Value);

app.UseXContentTypeOptions();
app.UseXfo(xfo => xfo.Deny());
app.UseXXssProtection(options => options.EnabledWithBlockMode());

app.MapFallbackToController("Index", "Home");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();