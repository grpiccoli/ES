using System.Globalization;

namespace EpicSolutions.Extensions
{
    public static class Statics
    {
        public const string DefaultCulture = "en";
        public static readonly CultureInfo[] SupportedCultures = new[]
        {
            new CultureInfo(DefaultCulture),
            new CultureInfo("es")
        };
    }
}
