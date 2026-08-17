using RepairTracker.Models;

namespace RepairTracker.Services;

public interface IWikiArticleService
{
    Task<List<WikiArticle>> GetAllAsync();
    Task<WikiArticle?> GetByIdAsync(string id);
    Task CreateAsync(WikiArticle article);
    Task UpdateAsync(WikiArticle article);
    Task DeleteAsync(string id);
}
