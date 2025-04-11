using System.Collections.Generic;

namespace FnbIdentity.STS.Identity.Configuration
{
    public class CultureConfiguration
    {
        public static readonly string[] AvailableCultures = { "en", "hi" };
        public static readonly string DefaultRequestCulture = "en";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<string> Cultures { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string DefaultCulture { get; set; } = DefaultRequestCulture;
    }
}
