using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.JSInterop;

namespace GenstarXKulayInventorySystem.Server.Services;

public class SalesHostedService : IHostedService, IDisposable
{
    private readonly ILogger<SalesHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer = null;
    private bool _isProcessing = false;

    public SalesHostedService(ILogger<SalesHostedService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesHostedService started.");

        // Run every 1 minute (delay: 0 sec, period: 60 sec)
        _timer = new Timer(ProcessInventory, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    public async void ProcessInventory(object? state)
    {
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;
            _logger.LogInformation("SalesHostedService is processing at: {time}", DateTimeOffset.Now);

            // Create a scope so you can resolve scoped services like DbContext
            using var scope = _scopeFactory.CreateScope();

            var salesItemService = scope.ServiceProvider.GetRequiredService<ISaleItemService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            // Example calls:
            List<SaleItemDto> sales = await salesItemService.GetAllUndeductedItemsAsync();
            if (sales.Count != 0)
            {
                bool result = false;
                ProductDto toBeUpdated = new();
                foreach (var item in sales)
                {
                    decimal subtractedValue = UtilitiesHelper.ConvertItems(
                                        item.Size ?? 0,
                                        item.Quantity,
                                        item.Product?.ProductMesurementOption
                                            ?? ProductsEnumHelpers.ProductMesurementOption.Gallon,
                                        item.UnitMeasurement);

                    // Deduct from current quantity safely
                    if (item.Product != null)
                    {
                        toBeUpdated = item.Product;
                        toBeUpdated.ActualQuantity = (item.Product.ActualQuantity) - subtractedValue;
                        result = await productService.UpdateAsync(toBeUpdated);
                        if (result)
                        {
                            _logger.LogInformation("Process successful");
                        }
                    }

                    else
                    {
                        foreach(var mixture in item.DataList)
                        {

                            toBeUpdated = await productService.GetByIdAsync(mixture.ProductId);

                            if (toBeUpdated is not null)
                            {
                                // ensure ProductMesurementOption has a value, otherwise default to Gallon
                                var measurementOption = toBeUpdated.ProductMesurementOption
                                                        ?? ProductsEnumHelpers.ProductMesurementOption.Gallon;

                                decimal paintQuantityValue = UtilitiesHelper.ConvertItems(
                                    mixture.Size ?? 0,
                                    1,
                                    measurementOption,
                                    mixture.UnitMeasurement);

                                toBeUpdated.ActualQuantity -= paintQuantityValue;

                                 result = await productService.UpdateAsync(toBeUpdated);
                                if (result)
                                {
                                    _logger.LogInformation("Process successful for ProductId {ProductId}", toBeUpdated.Id);
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to update product with Id {ProductId}", toBeUpdated.Id);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Product with Id {ProductId} not found", mixture.ProductId);
                            }
                        }

                    }
                    if (result)
                    {
                        bool resultUpdate = await salesItemService.UpdateSaleItemStatus(item);
                        if (resultUpdate)
                        {
                            _logger.LogInformation("Update sale item successfully");
                        }
                    }
                   
                    
                    
                }
            }
           // var products = await productService.GetAllAsync();

            _logger.LogInformation("Fetched {SalesCount} sales and products.", sales.Count);
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
