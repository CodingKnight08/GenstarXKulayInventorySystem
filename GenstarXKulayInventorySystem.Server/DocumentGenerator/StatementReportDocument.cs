using GenstarXKulayInventorySystem.Shared.DTOS;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Reports;

public class StatementReportDocument : IDocument
{
    private readonly StatementReportDocumentDto _model;

    public StatementReportDocument(StatementReportDocumentDto model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().PaddingTop(20).Text(x =>
            {
                x.Span("Generated on ");
                x.Span(_model.ReportGenerated.ToString("yyyy-MM-dd HH:mm"));
                x.Span(" | Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }

    // ================= HEADER =================
    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("STATEMENT OF ACCOUNT")
                .FontSize(20)
                .SemiBold()
                .AlignCenter();

            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Client: {_model.ClientName}").Bold();
                    c.Item().Text($"Address: {_model.Address}"); // Added client address
                });

                // Branch mapping
                string branchName = _model.Branch switch
                {
                    BranchOption.GeneralSantosCity => "Genstar Trade Paint Center",
                    BranchOption.Polomolok => "Kulay",
                    _ => _model.Branch.ToString()
                };

                row.RelativeItem().AlignRight()
                    .Text($"Branch: {branchName}");
            });
        });
    }

    // ================= CONTENT =================
    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            // Table with border for rows only
            column.Item().PaddingTop(20).Element(c => ComposeSalesTable(c));

            // Total Charged Sales below table aligned to Amount column
            var totalSales = _model.DailySales?.Sum(x => x.TotalAmount ?? 0m) ?? 0m;
            column.Item().PaddingTop(5).AlignRight().Text($"Total Charged Sales: ₱ {totalSales:N2}")
                  .FontSize(12)
                  .AlignRight();

            // Totals section (remaining and total balance)
            column.Item().PaddingTop(20).Element(c => ComposeTotals(c));

            // Optional notes
            if (!string.IsNullOrWhiteSpace(_model.Notes))
            {
                column.Item().PaddingTop(15)
                    .Text($"Notes: {_model.Notes}")
                    .FontSize(12);
            }
        });
    }

    // ================= SALES TABLE =================
    private void ComposeSalesTable(IContainer container)
    {
        container.Table(table =>
        {
            // Equal width columns
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(); // Date
                columns.RelativeColumn(); // Invoice
                columns.RelativeColumn(); // No. Items
                columns.RelativeColumn(); // Amount
            });

            // Table header (no borders)
            table.Header(header =>
            {
                header.Cell().Text("Date").Bold().AlignCenter();
                header.Cell().Text("Invoice").Bold().AlignCenter();
                header.Cell().Text("No. Items").Bold().AlignCenter();
                header.Cell().Text("Amount").Bold().AlignCenter();
            });

            // Table rows with borders, centered text
            foreach (var sale in _model.DailySales ?? Enumerable.Empty<DailySaleDto>())
            {
                table.Cell().Border(1).AlignCenter().Text(sale.DateOfSales.ToString("yyyy-MM-dd"));
                table.Cell().Border(1).AlignCenter()
                     .Text(!string.IsNullOrWhiteSpace(sale.RecieptReference)
                         ? sale.RecieptReference
                         : sale.SalesNumber);
                table.Cell().Border(1).AlignCenter().Text(sale.SaleItemsCount.ToString());
                table.Cell().Border(1).AlignCenter()
                     .Text($"₱ {(sale.TotalAmount ?? 0m).ToString("N2", CultureInfo.InvariantCulture)}");
            }
        });
    }

    // ================= TOTALS =================
    private void ComposeTotals(IContainer container)
    {
        container.AlignRight().Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Remaining Balance:");
                row.ConstantItem(120).AlignRight()
                    .Text($"₱ {_model.Credit:N2}")
                    .FontSize(12);
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Total Balance:").Bold();
                row.ConstantItem(120).AlignRight()
                    .Text($"₱ {_model.Balance:N2}")
                    .Bold()
                    .FontColor(_model.Balance < 0
                        ? Colors.Red.Darken4
                        : Colors.Black)
                    .FontSize(12);
            });
        });
    }
}
