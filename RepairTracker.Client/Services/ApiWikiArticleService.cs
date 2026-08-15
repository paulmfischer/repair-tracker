using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class ApiWikiArticleService(HttpClient http) : IWikiArticleService
{
    public async Task<List<WikiArticle>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<WikiArticle>>("api/wiki") ?? [];

    public async Task<WikiArticle?> GetByIdAsync(string id) =>
        await http.GetFromJsonAsync<WikiArticle>($"api/wiki/{id}");

    public async Task CreateAsync(WikiArticle article)
    {
        var response = await http.PostAsJsonAsync("api/wiki", article);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<WikiArticle>();
        if (created is not null)
        {
            article.Id = created.Id;
            article.CreatedAt = created.CreatedAt;
            article.UpdatedAt = created.UpdatedAt;
        }
    }

    public async Task UpdateAsync(WikiArticle article)
    {
        var response = await http.PutAsJsonAsync($"api/wiki/{article.Id}", article);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string id)
    {
        var response = await http.DeleteAsync($"api/wiki/{id}");
        response.EnsureSuccessStatusCode();
    }

    public Task<List<WikiFile>> UploadImagesAsync(string articleId, IReadOnlyList<IBrowserFile> files) =>
        UploadFilesAsync($"api/wiki/{articleId}/images", files);

    public Task<List<WikiFile>> UploadAttachmentsAsync(string articleId, IReadOnlyList<IBrowserFile> files) =>
        UploadFilesAsync($"api/wiki/{articleId}/attachments", files);

    private async Task<List<WikiFile>> UploadFilesAsync(string requestUri, IReadOnlyList<IBrowserFile> files)
    {
        using var content = new MultipartFormDataContent();
        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 25 * 1024 * 1024));
            content.Add(streamContent, "files", file.Name);
        }

        var response = await http.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WikiFile>>() ?? [];
    }

    public async Task DeleteFileAsync(string path)
    {
        var response = await http.DeleteAsync($"api/wiki/files?path={Uri.EscapeDataString(path)}");
        response.EnsureSuccessStatusCode();
    }
}
