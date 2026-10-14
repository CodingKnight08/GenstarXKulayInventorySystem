using AutoMapper;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class DashboardService : IDashboardService
{
    private readonly InventoryDbContext _dbContext;
    private readonly ILogger<DashboardService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public DashboardService(InventoryDbContext dbContext, ILogger<DashboardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }




    public async Task<DashBoardDto> GetDailySalesNetAndCOGS(
    BranchOption branch,
    DateTime date)
    {
        var philippineDate = PhilippineTime.ToPhilippineTime(date);

        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(philippineDate);


        var dailySales = await _dbContext.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.SaleItems)
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == branch &&
                x.DateOfSales >= startOfDay &&
                x.DateOfSales < endOfDay)
            .ToListAsync();

        var sales = _mapper.Map<List<DailySaleDto>>(dailySales);

        decimal totalNetSales = 0m;
        decimal totalProductCosts = 0m;

        foreach (var sale in sales)
        {
            // Add commission once per sale
            totalNetSales += sale.Commission ?? 0m;

            foreach (var item in sale.SaleItems)
            {
                // Selling price
                totalNetSales += item.ItemPrice * item.Quantity;

                if (item.DataList != null && item.DataList.Any())
                {
                    // MIX items
                    totalProductCosts += item.DataList.Sum(x =>
                        x.Quantity * x.CostPrice);
                }
                else
                {
                    // NON-MIX items
                    totalProductCosts += item.CostPrice * item.Quantity;
                }
            }
        }

        return new DashBoardDto
        {
            TotalNetSales = totalNetSales,
            TotalProductCosts = totalProductCosts,
            TotalProfit = totalNetSales - totalProductCosts,
            TotalDailySale = sales.Count,
            TotalItemsSold = sales.Sum(x => x.SaleItems.Count)
        };
    }
    public async Task<decimal> GetDailyExpenses(
    BranchOption branch,
    DateTime selectedDate)
    {
        BillingBranch billingBranch = branch switch
        {
            BranchOption.Polomolok => BillingBranch.Kulay,
            BranchOption.GeneralSantosCity => BillingBranch.GenStar,
            BranchOption.Warehouse => BillingBranch.Warehouse,
            _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
        };

        PurchaseShipToOption purchaseShipTo = branch switch
        {
            BranchOption.GeneralSantosCity => PurchaseShipToOption.GeneralSantosCity,
            BranchOption.Polomolok => PurchaseShipToOption.Polomolok,
            BranchOption.Warehouse => PurchaseShipToOption.Warehouse,
            _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
        };

        // Treat the selected date as a Philippine calendar date
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(selectedDate.Date);

        var billingTotal = await _dbContext.Billings
            .AsNoTracking()
            .Where(b =>
                !b.IsDeleted &&
                b.Branch == billingBranch &&
                b.DateOfBilling >= startOfDay &&
                b.DateOfBilling < endOfDay)
            .SumAsync(b => (decimal?)b.Amount) ?? 0m;

        var purchaseTotal = await _dbContext.PurchaseOrderItems
            .AsNoTracking()
            .Where(item =>
                item.PurchaseOrder != null &&
                !item.PurchaseOrder.IsDeleted &&
                item.PurchaseOrder.PurchaseShipToOption == purchaseShipTo &&
                item.PurchaseOrder.PurchaseOrderDate >= startOfDay &&
                item.PurchaseOrder.PurchaseOrderDate < endOfDay)
            .SumAsync(item => (decimal?)(item.ItemAmount * item.ItemQuantity)) ?? 0m;

        return billingTotal + purchaseTotal;
    }
    public async Task<List<SalesDto>> GetWeeklyExpenseProfitChart(
    BranchOption branch,
    DateTime date)
    {
        // Convert UTC date from UI/API to Philippine time
        var philippineDate = PhilippineTime.ToPhilippineTime(date).Date;

        // Get Monday as start of week
        var start = philippineDate.AddDays(
            -(int)philippineDate.DayOfWeek +
            (philippineDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

        var end = start.AddDays(7);

        return await GetSalesProfiChart(
            branch,
            start,
            end,
            d => d.ToString("ddd"),
            d => d.Date);
    }

    public async Task<List<ExpensesDto>> GetWeeklyExpenseProfitChartForReport(
     BranchOption branch,
     DateTime date)
    {
        var phDate = PhilippineTime.ToPH(date).Date;

        // Monday start of week
        var startOfWeek = phDate.AddDays(
            phDate.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - phDate.DayOfWeek
        );

        var endOfWeek = startOfWeek.AddDays(7);


        var billingBranch = branch switch
        {
            BranchOption.Polomolok => BillingBranch.Kulay,
            BranchOption.GeneralSantosCity => BillingBranch.GenStar,
            BranchOption.Warehouse => BillingBranch.Warehouse,
            _ => throw new ArgumentOutOfRangeException()
        };

        var purchaseBranch = branch switch
        {
            BranchOption.Polomolok => PurchaseShipToOption.Polomolok,
            BranchOption.GeneralSantosCity => PurchaseShipToOption.GeneralSantosCity,
            BranchOption.Warehouse => PurchaseShipToOption.Warehouse,
            _ => throw new ArgumentOutOfRangeException()
        };


        // Billing expenses
        var billingExpenses = await _dbContext.Billings
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == billingBranch &&
                x.DateOfBilling >= startOfWeek &&
                x.DateOfBilling < endOfWeek)
            .Select(x => new
            {
                Date = x.DateOfBilling.Date,
                Total = x.Amount
            })
            .ToListAsync();


        // Purchase order expenses
        var purchaseExpenses = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PurchaseShipToOption == purchaseBranch &&
                x.PurchaseOrderDate >= startOfWeek &&
                x.ExpectedDeliveryDate < endOfWeek)
            .Select(x => new
            {
                Date = x.PurchaseOrderDate.Date,
                Total = x.AssumeTotalAmount
            })
            .ToListAsync();


        // Combine expenses
        var allExpenses = billingExpenses
            .Concat(purchaseExpenses)
            .GroupBy(x => x.Date)
            .Select(x => new
            {
                Date = x.Key,
                Total = x.Sum(e => e.Total)
            })
            .ToList();


        // Generate complete 7 days
        var result = new List<ExpensesDto>();

        for (int i = 0; i < 7; i++)
        {
            var currentDate = startOfWeek.AddDays(i);

            result.Add(new ExpensesDto
            {
                Label = currentDate.ToString("ddd"),
                StartDate = currentDate,
                Total = allExpenses
                    .Where(x => x.Date == currentDate)
                    .Select(x => x.Total)
                    .FirstOrDefault()
            });
        }

        return result;
    }

    public async Task<List<ExpensesDto>> GetMonthlyExpenseProfitChartForReport(
     BranchOption branch,
     DateTime date)
    {
        var phDate = PhilippineTime.ToPH(date).Date;
        var startOfMonth = new DateTime(phDate.Year, phDate.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var billingBranch = branch switch
        {
            BranchOption.Polomolok => BillingBranch.Kulay,
            BranchOption.GeneralSantosCity => BillingBranch.GenStar,
            BranchOption.Warehouse => BillingBranch.Warehouse,
            _ => throw new ArgumentOutOfRangeException()
        };
        var purchaseBranch = branch switch
        {
            BranchOption.Polomolok => PurchaseShipToOption.Polomolok,
            BranchOption.GeneralSantosCity => PurchaseShipToOption.GeneralSantosCity,
            BranchOption.Warehouse => PurchaseShipToOption.Warehouse,
            _ => throw new ArgumentOutOfRangeException()
        };
        // Billing expenses
        var billingExpenses = await _dbContext.Billings
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == billingBranch &&
                x.DateOfBilling >= startOfMonth &&
                x.DateOfBilling < endOfMonth)
            .Select(x => new
            {
                Date = x.DateOfBilling.Date,
                Total = x.Amount
            })
            .ToListAsync();
        // Purchase order expenses
        var purchaseExpenses = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PurchaseShipToOption == purchaseBranch &&
                x.PurchaseOrderDate >= startOfMonth &&
                x.ExpectedDeliveryDate < endOfMonth)
            .Select(x => new
            {
                Date = x.PurchaseOrderDate.Date,
                Total = x.AssumeTotalAmount
            })
            .ToListAsync();
        // Combine expenses
        var allExpenses = billingExpenses
            .Concat(purchaseExpenses)
            .GroupBy(x => x.Date)
            .Select(x => new
            {
                Date = x.Key,
                Total = x.Sum(e => e.Total)
            })
            .ToList();
        // Generate complete days of the month
        var result = new List<ExpensesDto>();
        int daysInMonth = DateTime.DaysInMonth(phDate.Year, phDate.Month);

        for (int i = 1; i <= daysInMonth; i++)
        {
            var currentDate = new DateTime(phDate.Year, phDate.Month, i);

            result.Add(new ExpensesDto
            {
                Label = currentDate.ToString("ddd"),
                StartDate = currentDate,
                Total = allExpenses
                    .Where(x => x.Date == currentDate)
                    .Select(x => x.Total)
                    .FirstOrDefault()
            });
        }

        return result;
    }

    public async Task<List<SalesDto>> GetMonthlyExpenseProfitChart(
     BranchOption branch,
     DateTime date)
    {
        // Convert date received from UI/API to Philippine time
        var philippineDate = PhilippineTime.ToPhilippineTime(date).Date;

        var start = new DateTime(
            philippineDate.Year,
            philippineDate.Month,
            1);

        var end = start.AddMonths(1);

        return await GetSalesProfiChart(
            branch,
            start,
            end,
            d => d.ToString("dd"),
            d => d.Date);
    }
    public async Task<List<SalesDto>> GetYearlyExpenseProfitChart(BranchOption branch)
    {
        var today = PhilippineTime.Now.Date;

        var result = new List<SalesDto>();

        for (int month = 1; month <= 12; month++)
        {
            var start = new DateTime(today.Year, month, 1);
            var end = start.AddMonths(1);

            var data = await GetSalesProfiChart(
                branch,
                start,
                end,
                _ => start.ToString("MMM"),
                d => d.Date);

            result.Add(new SalesDto
            {
                Label = start.ToString("MMM"),
                StartDate = start,

                TotalSales = data.Sum(x => x.TotalSales),

                TotalProductCosts = data.Sum(x => x.TotalProductCosts)
            });
        }

        return result;
    }
    private async Task<List<SalesDto>> GetSalesProfiChart(
        BranchOption branch,
        DateTime startDate,
        DateTime endDate,
        Func<DateTime, string> labelSelector,
        Func<DateTime, DateTime> groupSelector)
    {
        var dailySales = await _dbContext.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.SaleItems)
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == branch &&
                x.DateOfSales >= startDate &&
                x.DateOfSales < endDate)
            .ToListAsync();


        var result = new List<SalesDto>();


        for (var current = startDate.Date; current < endDate.Date; current = current.AddDays(1))
        {
            var periodStart = groupSelector(current);
            var periodEnd = periodStart.AddDays(1);


            var sales = dailySales
                .Where(x =>
                    x.DateOfSales >= periodStart &&
                    x.DateOfSales < periodEnd)
                .ToList();


            decimal totalSales = 0;
            decimal landedCost = 0;


            foreach (var sale in sales)
            {
                // Commission included once per sale
                totalSales += sale.Commission ?? 0;


                foreach (var item in sale.SaleItems)
                {
                    // Sales amount
                    totalSales += item.ItemPrice * item.Quantity;


                    // Cost calculation
                    if (!string.IsNullOrWhiteSpace(item.DataList))
                    {
                        try
                        {
                            var mixItems = JsonSerializer.Deserialize<List<InvolvePaintsDto>>(item.DataList);

                            if (mixItems != null)
                            {
                                landedCost += mixItems.Sum(x =>
                                    x.Quantity * x.CostPrice);
                            }
                        }
                        catch
                        {
                            // fallback if invalid JSON
                            landedCost += item.CostPrice * item.Quantity;
                        }
                    }
                    else
                    {
                        landedCost += item.CostPrice * item.Quantity;
                    }
                }
            }


            result.Add(new SalesDto
            {
                Label = labelSelector(periodStart),
                StartDate = periodStart,
                TotalSales = totalSales,
                TotalProductCosts = landedCost
            });
        }


        return result;
    }
    public async Task<List<TopSaleItemDto>> GetWeeklyTopProducts(
     BranchOption branch,
     DateTime date)
    {
        // Convert selected UTC date from UI to Philippine time
        var philippineDate = PhilippineTime.ToPhilippineTime(date).Date;

        var startOfWeek = philippineDate.AddDays(
            -(int)philippineDate.DayOfWeek +
            (philippineDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

        var endOfWeek = startOfWeek.AddDays(7);


        var sales = await _dbContext.DailySales
            .AsNoTracking()
            .Include(x => x.SaleItems)
                .ThenInclude(x => x.BranchProduct)
                    .ThenInclude(x => x.MasterProduct)
                        .ThenInclude(x => x.ProductBrand)
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == branch &&
                x.DateOfSales >= startOfWeek &&
                x.DateOfSales < endOfWeek)
            .ToListAsync();


        var products = new List<TopSaleItemDto>();


        // Normal products
        products.AddRange(
            sales
                .SelectMany(x => x.SaleItems)
                .Where(x => x.BranchProduct?.MasterProduct != null)
                .Select(x => new TopSaleItemDto
                {
                    BrandName = x.BranchProduct.MasterProduct.ProductBrand?.BrandName
                                ?? "Unknown Brand",
                    ProductName = x.BranchProduct.MasterProduct.ProductName,
                    QuantitySold = x.Quantity,
                    TotalSales = x.ItemPrice * x.Quantity
                })
        );


        // Mixed products
        foreach (var sale in sales)
        {
            foreach (var item in sale.SaleItems)
            {
                if (string.IsNullOrEmpty(item.DataList))
                    continue;


                var mixedProducts = JsonSerializer.Deserialize<List<InvolvePaintsDto>>(
                    item.DataList);


                if (mixedProducts == null)
                    continue;


                foreach (var mixed in mixedProducts)
                {
                    products.Add(new TopSaleItemDto
                    {
                        BrandName = mixed.BrandName ?? "Unknown Brand",
                        ProductName = mixed.ProductName,
                        QuantitySold = mixed.Quantity,
                        TotalSales = mixed.Quantity * mixed.CostPrice
                    });
                }
            }
        }


        return products
            .GroupBy(x => new
            {
                x.BrandName,
                x.ProductName
            })
            .Select(g => new TopSaleItemDto
            {
                BrandName = g.Key.BrandName,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.QuantitySold),
                TotalSales = g.Sum(x => x.TotalSales)
            })
            .GroupBy(x => x.BrandName)
            .SelectMany(brandGroup => brandGroup
                .OrderByDescending(x => x.QuantitySold)
                .Take(10))
            .ToList();
    }
    public async Task<List<TopSaleItemDto>> GetMonthlyTopProducts(
    BranchOption branch,
    DateTime date)
    {
        // Convert the selected UTC date from the UI/API to Philippine time
        var philippineDate = PhilippineTime.ToPhilippineTime(date).Date;

        var startOfMonth = new DateTime(
            philippineDate.Year,
            philippineDate.Month,
            1);

        var endOfMonth = startOfMonth.AddMonths(1);

        var sales = await _dbContext.DailySales
            .AsNoTracking()
            .Include(x => x.SaleItems)
                .ThenInclude(x => x.BranchProduct)
                    .ThenInclude(x => x.MasterProduct)
                        .ThenInclude(x => x.ProductBrand)
            .Where(x =>
                !x.IsDeleted &&
                x.Branch == branch &&
                x.DateOfSales >= startOfMonth &&
                x.DateOfSales < endOfMonth)
            .ToListAsync();

        var products = new List<TopSaleItemDto>();

        // Normal products
        products.AddRange(
            sales
                .SelectMany(x => x.SaleItems)
                .Where(x => x.BranchProduct?.MasterProduct != null)
                .Select(x => new TopSaleItemDto
                {
                    BrandName = x.BranchProduct!.MasterProduct!.ProductBrand?.BrandName ?? "Unknown Brand",
                    ProductName = x.BranchProduct.MasterProduct.ProductName,
                    QuantitySold = x.Quantity,
                    TotalSales = x.ItemPrice * x.Quantity
                }));

        // Mixed products
        foreach (var sale in sales)
        {
            foreach (var item in sale.SaleItems)
            {
                if (string.IsNullOrWhiteSpace(item.DataList))
                    continue;

                var mixedProducts = JsonSerializer.Deserialize<List<InvolvePaintsDto>>(item.DataList);

                if (mixedProducts == null)
                    continue;

                foreach (var mixed in mixedProducts)
                {
                    products.Add(new TopSaleItemDto
                    {
                        BrandName = mixed.BrandName ?? "Unknown Brand",
                        ProductName = mixed.ProductName,
                        QuantitySold = mixed.Quantity,
                        TotalSales = mixed.Quantity * mixed.CostPrice
                    });
                }
            }
        }

        return products
            .GroupBy(x => new
            {
                x.BrandName,
                x.ProductName
            })
            .Select(g => new TopSaleItemDto
            {
                BrandName = g.Key.BrandName,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.QuantitySold),
                TotalSales = g.Sum(x => x.TotalSales)
            })
            .GroupBy(x => x.BrandName)
            .SelectMany(brandGroup => brandGroup
                .OrderByDescending(x => x.QuantitySold)
                .Take(5))
            .OrderBy(x => x.BrandName)
            .ThenByDescending(x => x.QuantitySold)
            .ToList();
    }
}

public interface IDashboardService
{
    Task<DashBoardDto> GetDailySalesNetAndCOGS(BranchOption branch,DateTime date);
    Task<decimal> GetDailyExpenses(
    BranchOption branch,
    DateTime utcDate);

    Task<List<ExpensesDto>> GetWeeklyExpenseProfitChartForReport(
     BranchOption branch,
     DateTime date);
    Task<List<ExpensesDto>> GetMonthlyExpenseProfitChartForReport(
     BranchOption branch,
     DateTime date);
    Task<List<SalesDto>> GetWeeklyExpenseProfitChart(
    BranchOption branch,
    DateTime date);
    Task<List<SalesDto>> GetMonthlyExpenseProfitChart(
     BranchOption branch,
     DateTime date);
    Task<List<SalesDto>> GetYearlyExpenseProfitChart(BranchOption branch);
    Task<List<TopSaleItemDto>> GetWeeklyTopProducts(
     BranchOption branch,
     DateTime date);
    Task<List<TopSaleItemDto>> GetMonthlyTopProducts(
    BranchOption branch,
    DateTime date);
}