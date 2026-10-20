using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Reports;

public class StatementReportDocument : IDocument
{
    private readonly StatementOfAccountDataDto _model;

    public StatementReportDocument(StatementOfAccountDataDto model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DateTime philTime = UtilitiesHelper.PhilippineTime.Now;
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
                
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }

    // ================= HEADER =================
    private void ComposeHeader(IContainer container)
    {
        var logoPath = GetLogoPath();

        string branchName = _model.Branch switch
        {
            BranchOption.GeneralSantosCity => "GENSTAR PAINT TRADE CENTER",
            BranchOption.Polomolok => "KULAY PAINT SUPPLY",
            _ => _model.Branch.ToString().ToUpper()
        };

        string streetAddress = _model.Branch switch
        {
            BranchOption.GeneralSantosCity => "Door 3, Napala Building Magsaysay Ave (Cor. Quirino)",
            BranchOption.Polomolok => "Pioneer Street, Poblacion",
            _ => string.Empty
        };

        string city = _model.Branch switch
        {
            BranchOption.GeneralSantosCity => "General Santos City",
            BranchOption.Polomolok => "Polomolok, South Cotabato",
            _ => string.Empty
        };

        string contactNumber = _model.Branch switch
        {
            BranchOption.GeneralSantosCity => "Contact No: (083) 553-20-26 or 0917-322-0503",
            BranchOption.Polomolok => "Contact No: 0967-942-0064",
            _ => string.Empty
        };

        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                // LEFT – LOGO (FIXED WIDTH)
                row.ConstantItem(80).AlignLeft().Element(c =>
                {
                    if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                    {
                        c.Height(70)
                         .Image(logoPath)
                         .FitArea();
                    }
                });

                // CENTER – TEXT
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    // Branch Name as H2 (Helvetica, larger font)
                    c.Item().Text(branchName)
                        .FontSize(20) // h2 size
                        .Bold()
                        .AlignCenter();

                    // Street Address
                    c.Item().Text(streetAddress)
                        .FontSize(11)
                        .AlignCenter();

                    // City below street address
                    if (!string.IsNullOrEmpty(city))
                    {
                        c.Item().Text(city)
                            .FontSize(11)
                            .AlignCenter();
                    }

                    // Contact Number
                    if (!string.IsNullOrEmpty(contactNumber))
                    {
                        c.Item().Text(contactNumber)
                            .FontSize(11)
                            .AlignCenter();
                    }

                    // Statement of Account title
                    c.Item().PaddingTop(6)
                        .Text("STATEMENT OF ACCOUNT")
                        .FontSize(14)
                        .SemiBold()
                        .AlignCenter();
                });

                // RIGHT – SPACER (MATCH LOGO WIDTH)
                row.ConstantItem(80);
            });

            // Divider
            column.Item()
                .PaddingTop(8)
                .LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten2);
        });
    }






    // ================= CONTENT =================
    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            // ===== CLIENT INFO ROW =====
            column.Item().PaddingTop(10).Row(row =>
            {
                // LEFT: To: ClientName
                row.RelativeItem().Text($"To: {_model.ClientName?.ToUpperInvariant()}")
                  .FontSize(12)
                  .Bold()
                  .AlignLeft();


                // RIGHT: Date of statement
                row.RelativeItem().Text($"Date: {philTime:MM/dd/yyyy}")
                    .FontSize(12)
                    .AlignRight();
            });

            // ===== GREETING =====
            column.Item().PaddingTop(5).Text("Dear Ma'am/Sir,")
                .FontSize(12)
                .AlignLeft();

            // ===== INTRO TEXT =====
            column.Item().PaddingTop(5).Text(
                    "We have summarized all \"Unpaid\" invoices and/or purchases that you had with us. Attached herewith are ALL ORIGINAL INVOICES. Please refer to the list below."
                )
                .FontSize(12)
                .AlignLeft();

            // ===== SALES TABLE =====
            column.Item().PaddingTop(15).Element(c => ComposeSalesTable(c));

            // ===== TOTAL CHARGED SALES =====
            decimal totalAmountDue = (_model.BeginningBalance) + (_model.ChargeSales?.Sum(x => x.TotalAmount ?? 0m) ?? 0m);

            column.Item().PaddingTop(5)
                .AlignRight()
                .Text(text =>
                {
                    text.Span("Amount Due: ").FontSize(13).Bold(); // Label normal
                    text.Span($"₱ {totalAmountDue:N2}")     // Total value bold & red
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.Red.Darken1).Underline();
                });


            // ===== FOOTER SECTION =====
            column.Item().PaddingTop(10).Column(footer =>
            {
                // Warning pattern - left aligned & italic
                footer.Item().Text("Should you have any clarification or verifications regarding the items presented above, please feel free to contact us via the contact numbers stated above. Thank you!")
                    .FontSize(10)
                    .FontColor(Colors.Red.Darken1)
                    .Italic()
                    .AlignLeft();

                footer.Item().PaddingTop(20).Row(row =>
                {
                    // Received By
                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("Received By:").FontSize(12);
                        r.RelativeItem()
                         .Height(15)
                         .AlignBottom()
                         .LineHorizontal(1)
                         .LineColor(Colors.Black);
                    });

                    row.ConstantItem(50); // Spacer

                    // Date
                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("Date:").FontSize(12);
                        r.RelativeItem()
                         .Height(15)
                         .AlignBottom()
                         .LineHorizontal(1)
                         .LineColor(Colors.Black);
                    });
                });





                footer.Item().PaddingTop(10).Text(
                        "\"THIS DOCUMENT IS NOT VALID FOR CLAIMING INPUT TAXES.\""
                    )

               .FontSize(10)
               .SemiBold()
               .Italic()
               .FontColor(Colors.Black)
               .AlignLeft();
            });


        });
    }


    // ================= SALES TABLE =================
    private void ComposeSalesTable(IContainer container)
    {
        container.Table(table =>
        {
            // 5 columns: Date | Transaction | No. Items | Amount | Balance
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(); // Date
                columns.RelativeColumn(); // Transaction
                columns.RelativeColumn(); // No. Items
                columns.RelativeColumn(); // Amount
                columns.RelativeColumn(); // Balance
            });

            // Table header
            table.Header(header =>
            {
                header.Cell().Text("Date").Bold().AlignCenter();
                header.Cell().Text("Transaction").Bold().AlignCenter();
                header.Cell().Text("No. of Items").Bold().AlignCenter();
                header.Cell().Text("Amount").Bold().AlignCenter();
                header.Cell().Text("Balance").Bold().AlignCenter();
            });

            // 1️⃣ Forward Balance
            var firstRowDate = _model.ChargeSales?.FirstOrDefault()?.DateOfSales.AddDays(-1)
                              ?? DateTime.Now.AddDays(-1);
            decimal runningBalance = _model.BeginningBalance;

            table.Cell().Border(1).AlignCenter().Text(firstRowDate.ToString("yyyy-MM-dd"));
            table.Cell().Border(1).AlignCenter().Text("Forward Balance");
            table.Cell().Border(1).AlignCenter().Text("-");
            table.Cell().Border(1).AlignCenter().Text("-");
            table.Cell().Border(1).AlignCenter().Text($"₱ {runningBalance:N2}");

            // 2️⃣ Charge Sales
            foreach (var sale in _model.ChargeSales ?? Enumerable.Empty<DailySaleDto>())
            {
                decimal saleAmount = sale.TotalAmount ?? 0m;
                runningBalance += saleAmount;

                table.Cell().Border(1).AlignCenter().Text(sale.DateOfSales.ToString("yyyy-MM-dd"));
                table.Cell().Border(1).AlignCenter()
                     .Text(!string.IsNullOrWhiteSpace(sale.RecieptReference)
                         ? $"INV # {sale.RecieptReference}"
                         : $"INV # {sale.SalesNumber}");
                table.Cell().Border(1).AlignCenter().Text(sale.SaleItemsCount.ToString());
                table.Cell().Border(1).AlignCenter().Text($"₱ {saleAmount:N2}");
                table.Cell().Border(1).AlignCenter().Text($"₱ {runningBalance:N2}");
            }

            // 3️⃣ Amount Due row in red
            var lastDayOfMonth = new DateTime(
                _model.ChargeSales?.FirstOrDefault()?.DateOfSales.Year ?? DateTime.Now.Year,
                _model.ChargeSales?.FirstOrDefault()?.DateOfSales.Month ?? DateTime.Now.Month,
                DateTime.DaysInMonth(
                    _model.ChargeSales?.FirstOrDefault()?.DateOfSales.Year ?? DateTime.Now.Year,
                    _model.ChargeSales?.FirstOrDefault()?.DateOfSales.Month ?? DateTime.Now.Month)
            );

            table.Cell().Border(1).AlignCenter().Text(lastDayOfMonth.ToString("yyyy-MM-dd"));
            table.Cell().Border(1).AlignCenter().Text("Amount Due");
            table.Cell().Border(1).AlignCenter().Text("-");
            table.Cell().Border(1).AlignCenter().Text("-");
            table.Cell().Border(1).AlignCenter().Text($"₱ {runningBalance:N2}");
        });
    }




    // ================= TOTALS =================

    private string GetLogoPath()
    {
        var basePath = AppContext.BaseDirectory;
        var wwwroot = Path.Combine(basePath, "wwwroot");

        return _model.Branch switch
        {
            BranchOption.GeneralSantosCity => Path.Combine(wwwroot, "Genstar.png"),
            BranchOption.Polomolok => Path.Combine(wwwroot, "Kulay.jpg"),
            _ => string.Empty
        };
    }
}
