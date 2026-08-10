using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace RepairTracker.Client.Services;

// Turns bare http(s) URLs inside free-text (repair notes) into clickable links,
// HTML-encoding everything else so user input can't inject markup.
public static partial class TextLinkifier
{
    [GeneratedRegex(@"https?://[^\s<>""']+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();

    public static bool IsHttpUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public static MarkupString ToMarkup(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new MarkupString(string.Empty);
        }

        var sb = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in UrlPattern().Matches(text))
        {
            var url = match.Value.TrimEnd('.', ',', ')', ']', '}', '!', '?', ';', ':');

            sb.Append(WebUtility.HtmlEncode(text[lastIndex..match.Index]));
            sb.Append($"<a href=\"{WebUtility.HtmlEncode(url)}\" target=\"_blank\" rel=\"noopener noreferrer\" style=\"color:var(--mud-palette-primary);\">{WebUtility.HtmlEncode(url)}</a>");
            sb.Append(WebUtility.HtmlEncode(text.Substring(match.Index + url.Length, match.Length - url.Length)));

            lastIndex = match.Index + match.Length;
        }

        sb.Append(WebUtility.HtmlEncode(text[lastIndex..]));

        return new MarkupString(sb.ToString());
    }
}
