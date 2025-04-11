using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FnbIdentity.STS.Identity.Helpers.TagHelpers
{
    [HtmlTargetElement("img-gravatar")]
    public class GravatarTagHelper : TagHelper
    {
        [HtmlAttributeName("email")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Email { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [HtmlAttributeName("alt")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Alt { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [HtmlAttributeName("class")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Class { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [HtmlAttributeName("size")]
        public int Size { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrWhiteSpace(Email))
            {
                var hash = Md5HashHelper.GetHash(Email);

                output.TagName = "img";
                if (!string.IsNullOrWhiteSpace(Class))
                {
                    output.Attributes.Add("class", Class);
                }

                if (!string.IsNullOrWhiteSpace(Alt))
                {
                    output.Attributes.Add("alt", Alt);
                }

                output.Attributes.Add("src", GetAvatarUrl(hash, Size));
                output.TagMode = TagMode.SelfClosing;
            }
        }

        private static string GetAvatarUrl(string hash, int size)
        {
            var sizeArg = size > 0 ? $"?s={size}" : "";

            return $"https://www.gravatar.com/avatar/{hash}{sizeArg}";
        }

    }
}
