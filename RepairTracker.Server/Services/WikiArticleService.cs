using MongoDB.Driver;
using RepairTracker.Data;
using RepairTracker.Models;

namespace RepairTracker.Services;

public class WikiArticleService : IWikiArticleService
{
    private readonly MongoDbContext _db;
    private readonly ILogger<WikiArticleService> _logger;

    public WikiArticleService(MongoDbContext db, ILogger<WikiArticleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<WikiArticle>> GetAllAsync() =>
        await _db.WikiArticles.Find(_ => true).SortBy(a => a.Category).ThenBy(a => a.Title).ToListAsync();

    public async Task<WikiArticle?> GetByIdAsync(string id)
    {
        var article = await _db.WikiArticles.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (article is null)
        {
            _logger.LogWarning("Wiki article {ArticleId} not found", id);
        }
        return article;
    }

    public async Task CreateAsync(WikiArticle article)
    {
        article.CreatedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        await _db.WikiArticles.ReplaceOneAsync(a => a.Id == article.Id, article, new ReplaceOptions { IsUpsert = true });
        _logger.LogInformation("Wiki article {ArticleId} created", article.Id);
    }

    public async Task UpdateAsync(WikiArticle article)
    {
        article.UpdatedAt = DateTime.UtcNow;
        await _db.WikiArticles.ReplaceOneAsync(a => a.Id == article.Id, article);
        _logger.LogInformation("Wiki article {ArticleId} updated", article.Id);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.WikiArticles.DeleteOneAsync(a => a.Id == id);
        _logger.LogInformation("Wiki article {ArticleId} deleted", id);
    }
}
