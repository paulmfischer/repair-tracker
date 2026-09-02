using RepairTracker.Models;

namespace RepairTracker.Server.Endpoints;

public static class WikiFilesEndpoints
{
    public static void MapWikiFilesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wiki/upload-limit", (IConfiguration config) =>
            Results.Ok(new { MaxFileSizeBytes = UploadsPath.GetMaxFileSizeBytes(config) }))
            .WithTags("Wiki Files")
            .WithSummary("Get the configured upload size limit")
            .WithDescription("Returns the max file size (bytes) the server currently accepts for wiki uploads, driven by Uploads:MaxFileSizeMb, so the client can enforce the same cap.")
            .Produces<object>();

        app.MapPost("/api/wiki/{articleId}/images", async (
            string articleId, IFormFileCollection files, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            var result = await SaveFilesAsync(articleId, "images", files, env, config);
            logger.LogInformation("Uploaded {FileCount} wiki image(s) for article {ArticleId}", result.Count, articleId);
            return Results.Ok(result);
        })
            .WithTags("Wiki Files")
            .WithSummary("Upload wiki images")
            .WithDescription("Saves one or more images for a wiki article and returns their stored file info, including relative paths under /uploads.")
            .Produces<List<WikiFile>>()
            .DisableAntiforgery();

        app.MapPost("/api/wiki/{articleId}/attachments", async (
            string articleId, IFormFileCollection files, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            var result = await SaveFilesAsync(articleId, "attachments", files, env, config);
            logger.LogInformation("Uploaded {FileCount} wiki attachment(s) for article {ArticleId}", result.Count, articleId);
            return Results.Ok(result);
        })
            .WithTags("Wiki Files")
            .WithSummary("Upload wiki attachments")
            .WithDescription("Saves one or more non-image attachments for a wiki article and returns their stored file info, including relative paths under /uploads.")
            .Produces<List<WikiFile>>()
            .DisableAntiforgery();

        app.MapDelete("/api/wiki/files", (string path, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            var uploadsRoot = UploadsPath.GetRoot(env, config);
            var relative = path.StartsWith("uploads/") ? path["uploads/".Length..] : path;
            var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relative));

            if (!UploadsPath.IsInsideRoot(fullPath, uploadsRoot))
            {
                logger.LogWarning("Rejected wiki file delete request outside uploads root: {Path}", path);
                return Results.BadRequest();
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Results.NoContent();
        })
            .WithTags("Wiki Files")
            .WithSummary("Delete a wiki file")
            .WithDescription("Deletes an uploaded wiki image or attachment by its /uploads-relative path. Rejects paths that resolve outside the uploads root.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<List<WikiFile>> SaveFilesAsync(
        string articleId, string kind, IFormFileCollection files, IWebHostEnvironment env, IConfiguration config)
    {
        var uploadDir = Path.Combine(UploadsPath.GetRoot(env, config), "wiki", articleId, kind);
        Directory.CreateDirectory(uploadDir);

        var result = new List<WikiFile>();
        foreach (var file in files)
        {
            var originalName = Path.GetFileName(file.FileName);
            var storedName = UniqueFileName(uploadDir, originalName);
            var dest = Path.Combine(uploadDir, storedName);

            await using (var stream = File.Create(dest))
            {
                await file.CopyToAsync(stream);
            }

            result.Add(new WikiFile
            {
                FileName = originalName,
                Path = $"uploads/wiki/{articleId}/{kind}/{storedName}",
                ContentType = file.ContentType
            });
        }

        return result;
    }

    // Files are scoped per-article, but the same article can accumulate multiple uploads with
    // the same original filename over time (e.g. re-uploading "manual.pdf") — suffix on collision
    // rather than overwrite, while WikiFile.FileName keeps the original name for display/download.
    private static string UniqueFileName(string uploadDir, string originalName)
    {
        if (!File.Exists(Path.Combine(uploadDir, originalName)))
        {
            return originalName;
        }

        var name = Path.GetFileNameWithoutExtension(originalName);
        var ext = Path.GetExtension(originalName);
        return $"{name}-{Guid.NewGuid().ToString()[..8]}{ext}";
    }
}
