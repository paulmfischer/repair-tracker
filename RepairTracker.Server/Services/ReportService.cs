using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RepairTracker.Models;
using RepairTracker.Server;

namespace RepairTracker.Services;

public class ReportService(IItemService itemService, IWebHostEnvironment env, IConfiguration config) : IReportService
{
    public async Task<byte[]?> GenerateItemReportAsync(string itemId)
    {
        var item = await itemService.GetByIdAsync(itemId);
        if (item is null || item.Status == RepairStatus.Intake)
        {
            return null;
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(item.Name).FontSize(18).Bold();
                    if (!string.IsNullOrWhiteSpace(item.SerialNumber))
                    {
                        column.Item().Text($"Serial Number: {item.SerialNumber}").FontSize(10);
                    }
                    if (!string.IsNullOrWhiteSpace(item.BoardVersion))
                    {
                        column.Item().Text($"Board Version: {item.BoardVersion}").FontSize(10);
                    }
                    if (!string.IsNullOrWhiteSpace(item.WarrantyStickerNumber))
                    {
                        column.Item().Text($"Warranty Sticker Number: {item.WarrantyStickerNumber}").FontSize(10);
                    }
                    column.Item().Text($"Generated on {DateTime.Now:MMM d, yyyy h:mm tt}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (item.PurchaseDate.HasValue)
                    {
                        column.Item().Text($"Purchase Date: {item.PurchaseDate.Value:MMM d, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                    if (item.SoldDate.HasValue)
                    {
                        column.Item().Text($"Sold Date: {item.SoldDate.Value:MMM d, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Column(column =>
                {
                    column.Spacing(10);

                    if (!string.IsNullOrWhiteSpace(item.GeneralNotes))
                    {
                        column.Item().Column(inner =>
                        {
                            inner.Item().Text("Notes").Bold().FontSize(12);
                            inner.Item().Text(item.GeneralNotes);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(item.OriginalFault))
                    {
                        column.Item().Column(inner =>
                        {
                            inner.Item().Text("Original Fault").Bold().FontSize(12);
                            inner.Item().Text(item.OriginalFault);
                        });
                    }

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    foreach (var status in Enum.GetValues<RepairStatus>())
                    {
                        var notes = item.Notes
                            .Where(n => n.StatusAtTime == status)
                            .OrderBy(n => n.CreatedAt)
                            .ToList();

                        if (notes.Count == 0) continue;

                        column.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text(status.ToDisplayString()).Bold().FontSize(14).FontColor(status.ToHex());
                            section.Item().LineHorizontal(1).LineColor(status.ToHex());

                            foreach (var note in notes)
                            {
                                section.Item().Column(noteColumn =>
                                {
                                    noteColumn.Spacing(4);
                                    noteColumn.Item().Text(note.CreatedAt.ToLocalTime().ToString("MMM d, yyyy h:mm tt"))
                                        .FontSize(9).FontColor(Colors.Grey.Darken1);

                                    if (!string.IsNullOrWhiteSpace(note.Text))
                                    {
                                        noteColumn.Item().Text(note.Text);
                                    }

                                    var images = LoadNoteImages(note);
                                    if (images.Count > 0)
                                    {
                                        noteColumn.Item().Column(imageRows =>
                                        {
                                            imageRows.Spacing(8);
                                            foreach (var rowImages in images.Chunk(3))
                                            {
                                                imageRows.Item().Row(row =>
                                                {
                                                    row.Spacing(8);
                                                    foreach (var imageBytes in rowImages)
                                                    {
                                                        row.RelativeItem().Image(imageBytes).FitWidth();
                                                    }
                                                    for (var i = rowImages.Length; i < 3; i++)
                                                    {
                                                        row.RelativeItem();
                                                    }
                                                });
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }

                    if (item.Status == RepairStatus.Parts)
                    {
                        column.Item().Column(inner =>
                        {
                            inner.Spacing(8);
                            inner.Item().Text("Moved to Parts Inventory").Bold().FontSize(14).FontColor(RepairStatus.Parts.ToHex());
                            inner.Item().LineHorizontal(1).LineColor(RepairStatus.Parts.ToHex());
                            inner.Item().Text($"This item was converted to Parts inventory on {item.UpdatedAt.ToLocalTime():MMM d, yyyy h:mm tt} and is no longer moving through the repair flow.")
                                .FontColor(Colors.Black);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private List<byte[]> LoadNoteImages(RepairNote note)
    {
        var images = new List<byte[]>();
        var uploadsRoot = GetUploadsRoot();

        foreach (var imagePath in note.ImagePaths)
        {
            var relative = imagePath.StartsWith("uploads/") ? imagePath["uploads/".Length..] : imagePath;
            var fullPath = Path.Combine(uploadsRoot, relative);
            if (File.Exists(fullPath))
            {
                images.Add(File.ReadAllBytes(fullPath));
            }
        }

        return images;
    }

    private string GetUploadsRoot() => UploadsPath.GetRoot(env, config);
}
