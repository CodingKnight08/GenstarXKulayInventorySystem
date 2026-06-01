using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GenstarXKulayInventorySystem.Client.Pages.Products;

public partial class ProductCsvAdder
{
    [Inject] private HttpClient Http { get; set; } = default!;
    private string ProductStatusMessage = "";
    private string BrandStatusMessage = "";
    private string BranchProductStatusMessage = "";
    private string BranchProductActualQtyStatusMessage = "";
    private string BranchProductPriceStatusMessage = "";

    // ---------------- Upload Products ----------------
    private async Task UploadFile(IBrowserFile file, string url, Action<string> setMessage)
    {
        try
        {
            if (file == null)
            {
                setMessage("❌ No file selected.");
                return;
            }

            using var content = new MultipartFormDataContent();

            content.Add(
                new StreamContent(file.OpenReadStream(50 * 1024 * 1024)),
                "file",
                file.Name);

            var response = await Http.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            setMessage(response.IsSuccessStatusCode
                ? $"✅ {result}"
                : $"❌ {result}");
        }
        catch (Exception ex)
        {
            setMessage($"❌ Error: {ex.Message}");
        }
    }

    // ==============================
    // 📦 Products (if you have endpoint)
    // ==============================
    private async Task UploadProductFile(InputFileChangeEventArgs e)
    {
        await UploadFile(e.File, "api/productimport/upload", msg =>
            ProductStatusMessage = msg);
    }

    // ==============================
    // 🏷️ Brands
    // ==============================
    private async Task UploadBrandFile(InputFileChangeEventArgs e)
    {
        await UploadFile(e.File, "api/productimport/upload-brands", msg =>
            BrandStatusMessage = msg);
    }

    // ==============================
    // 📦 Branch Products
    // ==============================
    private async Task UploadBranchProductFile(InputFileChangeEventArgs e)
    {
        await UploadFile(e.File, "api/productimport/upload-branchproducts", msg =>
            BranchProductStatusMessage = msg);
    }

    // ==============================
    // 📊 Actual Quantity Update
    // ==============================
    private async Task UploadBranchProductActualQuantityFile(InputFileChangeEventArgs e)
    {
        await UploadFile(e.File, "api/productimport/update-branchproducts-actualquantity", msg =>
            BranchProductActualQtyStatusMessage = msg);
    }

    // ==============================
    // 💰 Price Update (NEW)
    // ==============================
    private async Task UploadBranchProductPriceFile(InputFileChangeEventArgs e)
    {
        await UploadFile(e.File, "api/productimport/update-branchproducts-prices", msg =>
            BranchProductPriceStatusMessage = msg);
    }
}
