using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class WikiEndpoints
{
    public static void MapWikiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wiki").WithTags("Wiki");

        group.MapGet("/", async (IWikiArticleService wikiService) =>
            Results.Ok(await wikiService.GetAllAsync()))
            .WithSummary("List wiki articles")
            .WithDescription("Returns every wiki article.")
            .Produces<List<WikiArticle>>();

        group.MapGet("/{id}", async (string id, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching wiki article {ArticleId}", id);
            var article = await wikiService.GetByIdAsync(id);
            return article is null ? Results.NotFound() : Results.Ok(article);
        })
            .WithSummary("Get a wiki article")
            .WithDescription("Returns a single wiki article by id.")
            .Produces<WikiArticle>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (WikiArticle article, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating wiki article {ArticleId}", article.Id);
            await wikiService.CreateAsync(article);
            return Results.Ok(article);
        })
            .WithSummary("Create a wiki article")
            .WithDescription("Creates a new wiki article. The id is minted by the caller.")
            .Produces<WikiArticle>();

        group.MapPut("/{id}", async (string id, WikiArticle article, IWikiArticleService wikiService, ILogger<Program> logger) =>
        {
            article.Id = id;
            logger.LogInformation("Updating wiki article {ArticleId}", id);
            await wikiService.UpdateAsync(article);
            return Results.NoContent();
        })
            .WithSummary("Update a wiki article")
            .WithDescription("Replaces an existing wiki article's fields.")
            .Produces(StatusCodes.Status204NoContent);

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
        })
            .WithSummary("Delete a wiki article")
            .WithDescription("Deletes a wiki article by id and removes its uploaded images/attachments from disk.")
            .Produces(StatusCodes.Status204NoContent);
    }
}
