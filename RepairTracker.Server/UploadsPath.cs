namespace RepairTracker.Server;

public static class UploadsPath
{
    // Deliberately outside wwwroot: files written here at runtime must not sit inside the
    // Static Web Assets-watched tree, or the dev-time file watcher treats them as a source
    // change and forces a full browser reload mid-upload.
    public static string GetRoot(IWebHostEnvironment env, IConfiguration config) =>
        config["Uploads:Path"] is { Length: > 0 } p ? p : Path.Combine(env.ContentRootPath, "uploads");

    // Guards delete/serve requests built from a client-supplied relative path against traversing
    // outside the uploads root (e.g. via "../" segments).
    public static bool IsInsideRoot(string fullPath, string uploadsRoot) =>
        fullPath.StartsWith(Path.GetFullPath(uploadsRoot), StringComparison.Ordinal);

    // Also drives Kestrel's MaxRequestBodySize and the multipart form parser's length limit in
    // Program.cs, and is surfaced to the WASM client via GET /api/wiki/upload-limit so the
    // client-side OpenReadStream cap in ApiWikiArticleService stays in sync without a rebuild.
    public static long GetMaxFileSizeBytes(IConfiguration config) =>
        config.GetValue("Uploads:MaxFileSizeMb", 200) * 1024L * 1024L;
}
