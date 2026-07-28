namespace RepairTracker.Server.Endpoints;

public static class ImagesEndpoints
{
    public static void MapImagesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/items/{itemId}/notes/{noteId}/images", async (
            string itemId, string noteId, IFormFileCollection files, IWebHostEnvironment env, IConfiguration config) =>
        {
            var uploadDir = Path.Combine(UploadsPath.GetRoot(env, config), itemId, noteId);
            Directory.CreateDirectory(uploadDir);

            var paths = new List<string>();
            foreach (var file in files)
            {
                var safeName = Path.GetFileName(file.FileName);
                var dest = Path.Combine(uploadDir, safeName);
                await using var stream = File.Create(dest);
                await file.CopyToAsync(stream);
                paths.Add($"uploads/{itemId}/{noteId}/{safeName}");
            }

            return Results.Ok(paths);
        }).DisableAntiforgery();

        app.MapDelete("/api/images", (string path, IWebHostEnvironment env, IConfiguration config) =>
        {
            var uploadsRoot = UploadsPath.GetRoot(env, config);
            var relative = path.StartsWith("uploads/") ? path["uploads/".Length..] : path;
            var fullPath = Path.GetFullPath(Path.Combine(uploadsRoot, relative));

            if (!fullPath.StartsWith(Path.GetFullPath(uploadsRoot), StringComparison.Ordinal))
            {
                return Results.BadRequest();
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Results.NoContent();
        });
    }
}
