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
}
