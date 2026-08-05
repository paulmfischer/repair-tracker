using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RepairTracker.Server;

public static class ThumbnailGenerator
{
    public const int MaxDimension = 200;

    public static async Task GenerateAsync(string sourcePath, string destPath)
    {
        using var image = await Image.LoadAsync(sourcePath);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(MaxDimension, MaxDimension)
        }));

        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        await image.SaveAsync(destPath);
    }
}
