using GenstarXKulayInventorySystem.Shared.DTOS;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.Helpers;

public static class UtilitiesHelper
{
    public static DateTime GetPhilippineTime()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }
    public static DateTime ConvertPhilippineToUtc(DateTime philippineTime)
    {
        TimeZoneInfo phZone;

        try
        {
            phZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        }
        catch (TimeZoneNotFoundException)
        {
            phZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        }

        return TimeZoneInfo.ConvertTimeToUtc(philippineTime, phZone);
    }


    private static readonly TimeZoneInfo PhilippineTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"); // PHT is same as Singapore Standard Time

    public static DateTime ConvertUtcToPhilippineTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Unspecified)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, PhilippineTimeZone);
    }

    public static decimal ConvertItems(
     decimal size,
     int quantity,
     ProductMesurementOption productUnit,
     ProductMesurementOption saleItemUnit)
    {
        decimal totalSize = size * quantity;

        // Volume conversions
        if (IsVolume(productUnit) && IsVolume(saleItemUnit))
        {
            return (productUnit, saleItemUnit) switch
            {
                (ProductMesurementOption.Gallon, ProductMesurementOption.Milliliter) => totalSize / 3785m,
                (ProductMesurementOption.Gallon, ProductMesurementOption.Gallon) => totalSize,
                (ProductMesurementOption.Liter, ProductMesurementOption.Milliliter) => totalSize / 1000m,
                (ProductMesurementOption.Quart, ProductMesurementOption.Milliliter) => totalSize / 946m,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Milliliter) => totalSize,

                (ProductMesurementOption.Milliliter, ProductMesurementOption.Gallon) => totalSize * 3785m,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Liter) => totalSize * 1000m,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Quart) => totalSize * 946m,
                (ProductMesurementOption.Liter, ProductMesurementOption.Quart) => totalSize * 1.057m,
                (ProductMesurementOption.Liter, ProductMesurementOption.Liter) => totalSize,
                (ProductMesurementOption.Quart, ProductMesurementOption.Liter) => totalSize * 0.946m,
                (ProductMesurementOption.Quart, ProductMesurementOption.Quart) => totalSize,
                (ProductMesurementOption.Quart, ProductMesurementOption.Gallon) => totalSize * 0.25m,
                _ => throw new Exception($"No conversion available for {productUnit} -> {saleItemUnit}")
            };
        }

        // Length conversions
        if (IsLength(productUnit) && IsLength(saleItemUnit))
        {
            return (productUnit, saleItemUnit) switch
            {
                (ProductMesurementOption.Yard, ProductMesurementOption.Feet) => totalSize * 3m,
                (ProductMesurementOption.Feet, ProductMesurementOption.Yard) => totalSize / 3m,

                (ProductMesurementOption.Meter, ProductMesurementOption.Feet) => totalSize * 3.28084m,
                (ProductMesurementOption.Feet, ProductMesurementOption.Meter) => totalSize * 0.3048m,

                (ProductMesurementOption.Meter, ProductMesurementOption.Yard) => totalSize * 1.09361m,
                (ProductMesurementOption.Yard, ProductMesurementOption.Meter) => totalSize * 0.9144m,

                _ => throw new Exception($"No conversion available for {productUnit} -> {saleItemUnit}")
            };
        }

        return totalSize;
    }

    public static BillingBranch GetBillingBranch(BranchOption branchOption)
    {
        return branchOption switch
        {
            BranchOption.GeneralSantosCity => BillingBranch.GenStar,
            BranchOption.Polomolok => BillingBranch.Kulay,
            BranchOption.Warehouse => BillingBranch.Warehouse,
            _ => throw new ArgumentOutOfRangeException(nameof(branchOption), branchOption, null)
        };
    }

    public static PurchaseShipToOption GetPurchaseToShipOption (BranchOption branchOption) {
        return branchOption switch
        {

            BranchOption.GeneralSantosCity => PurchaseShipToOption.GeneralSantosCity,
            BranchOption.Polomolok => PurchaseShipToOption.Polomolok,
            BranchOption.Warehouse => PurchaseShipToOption.Warehouse,
            _ => throw new ArgumentOutOfRangeException(nameof(branchOption), branchOption, null)
        };
            }

    private static bool IsVolume(ProductMesurementOption unit) =>
        unit == ProductMesurementOption.Gallon ||
        unit == ProductMesurementOption.Liter ||
        unit == ProductMesurementOption.Quart ||
        unit == ProductMesurementOption.Milliliter;

    private static bool IsLength(ProductMesurementOption unit) =>
        unit == ProductMesurementOption.Yard ||
        unit == ProductMesurementOption.Feet ||
        unit == ProductMesurementOption.Meter;



    public enum PaymentMethod
    {
        Cash,
        GCash,
        CreditCard,
        DebitCard,
        BankTransfer,
        MobilePayment,
        BankCheque,
        Other
    }

    public enum DateRangeOption
    {
        OneWeek = 1,
        OneMonth = 2,
        TwoMonths = 3,
        ThreeMonths = 4,
        OneYear = 5
    }
    
}
