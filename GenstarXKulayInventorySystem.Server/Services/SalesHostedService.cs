using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.JSInterop;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

public class SalesHostedService : IHostedService, IDisposable
{
    private readonly ILogger<SalesHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer = null;
    private bool _isProcessing = false;
    private readonly IConfiguration _configuration;

    public SalesHostedService(ILogger<SalesHostedService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var isEnabled = _configuration.GetValue<bool>(
            "BackgroundServices:EnableSalesHostedService");


        if (!isEnabled)
        {
            _logger.LogInformation("SalesHostedService is disabled by configuration.");
            return Task.CompletedTask;
        }


        _logger.LogInformation("SalesHostedService started.");

        _timer = new Timer(
            ProcessInventory,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(2));

        return Task.CompletedTask;
    }

    public async void ProcessInventory(object? state)
    {
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;
            _logger.LogInformation("SalesHostedService is processing at: {time}", DateTimeOffset.Now);

            using var scope = _scopeFactory.CreateScope();

            var salesItemService = scope.ServiceProvider.GetRequiredService<ISaleItemService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            //BranchOption branch = BranchOption.Polomolok;

            var sales = await salesItemService.GetAllUndeductedItemsAsync();

            if (sales.Count == 0)
            {
                _logger.LogInformation("No undeducted sales items found.");
                return;
            }

            foreach (var item in sales)
            {
                bool itemSuccess = true;

                // ✅ DIRECT COMPUTATION (NO CONVERSION)
                decimal baseQuantity = item.Quantity * item.Size.GetValueOrDefault(1);

                // =========================
                // NORMAL PRODUCT
                // =========================
                if (item.PaintCategory != PaintCategory.Mix)
                {
                    var result = await productService.UpdateSaleItemProduct(item.BranchProductId, baseQuantity);

                    if (!result)
                    {
                        itemSuccess = false;
                        _logger.LogWarning("Failed to update product with Id {ProductId}", item.BranchProductId);
                    }
                    else
                    {
                        _logger.LogInformation("Deducted {Qty} from ProductId {ProductId}", baseQuantity, item.BranchProductId);
                    }
                }
                // =========================
                // MIXTURE PRODUCTS
                // =========================
                else
                {
                    foreach (var mixture in item.DataList)
                    {
                        decimal mixtureQty = (mixture.Size ?? 1) * 1 * mixture.Quantity; // always 1 qty per mixture item

                        var result = await productService.UpdateSaleItemProduct(mixture.ProductId, mixtureQty);

                        if (!result)
                        {
                            itemSuccess = false;
                            _logger.LogWarning("Failed to update mixture ProductId {ProductId}", mixture.ProductId);
                        }
                        else
                        {
                            _logger.LogInformation("Deducted {Qty} from mixture ProductId {ProductId}", mixtureQty, mixture.ProductId);
                        }
                    }
                }

                // =========================
                // UPDATE SALE ITEM STATUS
                // =========================
                if (itemSuccess)
                {
                    var updated = await salesItemService.UpdateSaleItemStatus(item.Id);

                    if (updated)
                    {
                        _logger.LogInformation("Sale item {SaleId} marked as deducted", item.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update sale item status for {SaleId}", item.Id);
                    }
                }
            }

            _logger.LogInformation("Processed {SalesCount} sales.", sales.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing SalesHostedService.");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesHostedService is stopping.");

        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
