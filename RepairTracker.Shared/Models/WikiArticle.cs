using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepairTracker.Models;

public class WikiArticle
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Title { get; set; } = string.Empty;
    public string MarkdownBody { get; set; } = string.Empty;

    // Slash-delimited virtual folder path, e.g. "Nintendo/Switch". Empty = uncategorized/root.
    public string Category { get; set; } = string.Empty;

    // Flat, cross-cutting labels independent of the Category tree hierarchy.
    public List<string> Tags { get; set; } = new();

    public List<WikiFile> Images { get; set; } = new();
    public List<WikiFile> Attachments { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WikiFile
{
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    [BsonIgnore]
    public bool IsBrowserViewable => ContentType.StartsWith("image/") || ContentType == "application/pdf";
}
