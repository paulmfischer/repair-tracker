using Markdig;
using Microsoft.AspNetCore.Components;

namespace RepairTracker.Client.Services;

// Renders wiki article markdown to HTML entirely client-side — the server only ever
// stores/returns raw MarkdownBody text. No sanitizer: single-user app, only the owner
// ever writes article content, same trust model as TextLinkifier's note-text rendering.
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    public static MarkupString ToMarkup(string? markdown) =>
        new(Markdig.Markdown.ToHtml(markdown ?? string.Empty, Pipeline));
}
