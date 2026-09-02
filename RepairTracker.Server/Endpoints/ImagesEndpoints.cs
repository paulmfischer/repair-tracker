using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class ImagesEndpoints
{
    public static void MapImagesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/items/{itemId}/notes/{noteId}/images", async (
            string itemId, string noteId, IFormFileCollection files, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            logger.LogInformation("Uploading {FileCount} image(s) for item {ItemId}, note {NoteId}", files.Count, itemId, noteId);
            var uploadDir = Path.Combine(UploadsPath.GetRoot(env, config), itemId, noteId);
            Directory.CreateDirectory(uploadDir);
            var thumbDir = Path.Combine(uploadDir, "thumbs");

            var paths = new List<string>();
            var thumbPaths = new List<string>();
            foreach (var file in files)
            {
                var safeName = Path.GetFileName(file.FileName);
                var dest = Path.Combine(uploadDir, safeName);
                await using (var stream = File.Create(dest))
                {
                    await file.CopyToAsync(stream);
                }
                paths.Add($"uploads/{itemId}/{noteId}/{safeName}");

                var thumbDest = Path.Combine(thumbDir, safeName);
                await ThumbnailGenerator.GenerateAsync(dest, thumbDest);
                thumbPaths.Add($"uploads/{itemId}/{noteId}/thumbs/{safeName}");
            }

            logger.LogInformation("Uploaded {FileCount} image(s) for item {ItemId}, note {NoteId}", paths.Count, itemId, noteId);
            return Results.Ok(new ImageUploadResult(paths, thumbPaths));
        })
            .WithTags("Images")
            .WithSummary("Upload note images")
            .WithDescription("Saves one or more images for a repair note, generating a thumbnail for each, and returns their relative paths under /uploads.")
            .Produces<ImageUploadResult>()
            .DisableAntiforgery();

        app.MapDelete("/api/images", (string path, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            var uploadsRoot = UploadsPath.GetRoot(env, config);
            var relative = path.StartsWith("uploads/") ? path["uploads/".Length..] : path;
            var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relative));

            if (!UploadsPath.IsInsideRoot(fullPath, uploadsRoot))
            {
                logger.LogWarning("Rejected image delete request outside uploads root: {Path}", path);
                return Results.BadRequest();
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Also remove the sibling thumbnail: uploads/{itemId}/{noteId}/{file} -> uploads/{itemId}/{noteId}/thumbs/{file}
            var lastSlash = path.LastIndexOf('/');
            if (lastSlash >= 0)
            {
                var thumbPath = $"{path[..lastSlash]}/thumbs/{path[(lastSlash + 1)..]}";
                DeleteIfInsideUploadsRoot(thumbPath, uploadsRoot);
            }

            return Results.NoContent();
        })
            .WithTags("Images")
            .WithSummary("Delete a note image")
            .WithDescription("Deletes an uploaded note image (and its sibling thumbnail, if any) by its /uploads-relative path. Rejects paths that resolve outside the uploads root.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/images/backfill-thumbnails", async (
            ItemService itemService, IWebHostEnvironment env, IConfiguration config) =>
        {
            var uploadsRoot = UploadsPath.GetRoot(env, config);
            var items = await itemService.GetItemsNeedingThumbnailsAsync();

            var itemsUpdated = 0;
            var notesUpdated = 0;
            var thumbnailsGenerated = 0;
            var failures = new List<string>();

            foreach (var item in items)
            {
                var itemChanged = false;

                foreach (var note in item.Notes)
                {
                    if (note.ImagePaths.Count == note.ThumbnailPaths.Count)
                    {
                        continue;
                    }

                    while (note.ThumbnailPaths.Count < note.ImagePaths.Count)
                    {
                        var index = note.ThumbnailPaths.Count;
                        var fileName = Path.GetFileName(note.ImagePaths[index]);
                        var sourcePath = Path.Combine(uploadsRoot, item.Id, note.Id, fileName);

                        if (!File.Exists(sourcePath))
                        {
                            failures.Add($"item {item.Id}, note {note.Id}: missing source file {fileName}");
                            note.ThumbnailPaths.Add(string.Empty);
                            continue;
                        }

                        var thumbDest = Path.Combine(uploadsRoot, item.Id, note.Id, "thumbs", fileName);
                        await ThumbnailGenerator.GenerateAsync(sourcePath, thumbDest);
                        note.ThumbnailPaths.Add($"uploads/{item.Id}/{note.Id}/thumbs/{fileName}");
                        thumbnailsGenerated++;
                    }

                    notesUpdated++;
                    itemChanged = true;
                }

                if (itemChanged)
                {
                    await itemService.UpdateAsync(item);
                    itemsUpdated++;
                }
            }

            return Results.Ok(new { itemsUpdated, notesUpdated, thumbnailsGenerated, failures });
        })
            .WithTags("Images")
            .WithSummary("Backfill missing thumbnails")
            .WithDescription("One-off maintenance endpoint: generates thumbnails for any existing note images that predate thumbnail support.");
    }

    private static void DeleteIfInsideUploadsRoot(string relativeOrPrefixedPath, string uploadsRoot)
    {
        var relative = relativeOrPrefixedPath.StartsWith("uploads/") ? relativeOrPrefixedPath["uploads/".Length..] : relativeOrPrefixedPath;
        var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relative));

        if (!UploadsPath.IsInsideRoot(fullPath, uploadsRoot))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
