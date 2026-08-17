using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class WikiEndpoints
{
    public static void MapWikiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wiki");

        group.MapGet("/", async (IWikiArticleService wikiService) =>
            Results.Ok(await wikiService.GetAllAsync()));

        group.MapGet("/{id}", async (string id, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching wiki article {ArticleId}", id);
            var article = await wikiService.GetByIdAsync(id);
            return article is null ? Results.NotFound() : Results.Ok(article);
        });

        group.MapPost("/", async (WikiArticle article, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating wiki article {ArticleId}", article.Id);
            await wikiService.CreateAsync(article);
            return Results.Ok(article);
        });

        group.MapPut("/{id}", async (string id, WikiArticle article, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            article.Id = id;
            logger.LogInformation("Updating wiki article {ArticleId}", id);
            await wikiService.UpdateAsync(article);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (
            string id, IWikiArticleService wikiService, IWebHostEnvironment env, IConfiguration config, ILogger<Program> logger) =>
        {
            logger.LogInformation("Deleting wiki article {ArticleId}", id);
            await wikiService.DeleteAsync(id);

            var articleDir = Path.Combine(UploadsPath.GetRoot(env, config), "wiki", id);
            if (Directory.Exists(articleDir))
            {
                Directory.Delete(articleDir, recursive: true);
            }

            return Results.NoContent();
        });
    }
}
