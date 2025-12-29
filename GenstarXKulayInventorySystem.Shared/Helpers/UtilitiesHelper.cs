using GenstarXKulayInventorySystem.Shared.DTOS;
using System.Drawing;
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
    public static class PhilippineTime
    {
        private static readonly TimeZoneInfo PhZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhZone);

        public static DateTime ToPH(DateTime utcOrLocal) =>
            utcOrLocal.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(utcOrLocal, PhZone)
                : TimeZoneInfo.ConvertTime(utcOrLocal, PhZone);
        public static (DateTime StartOfDay, DateTime EndOfDay) GetDayRange(DateTime date)
        {
            var phDate = ToPH(date);
            var start = phDate.Date;
            var end = start.AddDays(1);
            return (start, end);
        }
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
       decimal quantity,
       ProductMesurementOption productUnit,
       ProductMesurementOption saleItemUnit)
    {
        // Combine the base volume first
        decimal totalBaseValue = size * quantity;

        // Volume conversions
        if (IsVolume(productUnit) && IsVolume(saleItemUnit))
        {
            // Convert productUnit to saleItemUnit
            return (productUnit, saleItemUnit) switch
            {
                // Gallon conversions
                (ProductMesurementOption.Gallon, ProductMesurementOption.Milliliter) => totalBaseValue * 3785m,
                (ProductMesurementOption.Gallon, ProductMesurementOption.Liter) => totalBaseValue * 3.785m,
                (ProductMesurementOption.Gallon, ProductMesurementOption.Quart) => totalBaseValue * 4m,
                (ProductMesurementOption.Gallon, ProductMesurementOption.Gallon) => totalBaseValue,

                // Liter conversions
                (ProductMesurementOption.Liter, ProductMesurementOption.Milliliter) => totalBaseValue * 1000m,
                (ProductMesurementOption.Liter, ProductMesurementOption.Gallon) => totalBaseValue / 3.785m,
                (ProductMesurementOption.Liter, ProductMesurementOption.Quart) => totalBaseValue * 1.057m,
                (ProductMesurementOption.Liter, ProductMesurementOption.Liter) => totalBaseValue,

                // Quart conversions
                (ProductMesurementOption.Quart, ProductMesurementOption.Milliliter) => totalBaseValue * 946.4m,
                (ProductMesurementOption.Quart, ProductMesurementOption.Liter) => totalBaseValue * 0.946m,
                (ProductMesurementOption.Quart, ProductMesurementOption.Gallon) => totalBaseValue * 0.25m,
                (ProductMesurementOption.Quart, ProductMesurementOption.Quart) => totalBaseValue,

                // Milliliter conversions
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Gallon) => totalBaseValue / 3785m,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Liter) => totalBaseValue / 1000m,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Quart) => totalBaseValue / 946.4m
,
                (ProductMesurementOption.Milliliter, ProductMesurementOption.Milliliter) => totalBaseValue,

                _ => throw new Exception($"No conversion available for {productUnit} -> {saleItemUnit}")
            };
        }

        // Length conversions
        if (IsLength(productUnit) && IsLength(saleItemUnit))
        {
            return (productUnit, saleItemUnit) switch
            {
                (ProductMesurementOption.Yard, ProductMesurementOption.Feet) => totalBaseValue * 3m,
                (ProductMesurementOption.Feet, ProductMesurementOption.Yard) => totalBaseValue / 3m,

                (ProductMesurementOption.Meter, ProductMesurementOption.Feet) => totalBaseValue * 3.28084m,
                (ProductMesurementOption.Feet, ProductMesurementOption.Meter) => totalBaseValue * 0.3048m,

                (ProductMesurementOption.Meter, ProductMesurementOption.Yard) => totalBaseValue * 1.09361m,
                (ProductMesurementOption.Yard, ProductMesurementOption.Meter) => totalBaseValue * 0.9144m,

                _ => throw new Exception($"No conversion available for {productUnit} -> {saleItemUnit}")
            };
        }

        // Default — no conversion rule
        return totalBaseValue;
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


    public static string GetStatusColorKey(DeliveryStatusOption status) => status switch
    {
        DeliveryStatusOption.Pending => "warning",
        DeliveryStatusOption.OnTheWay => "info",
        DeliveryStatusOption.Delivered => "success",
        DeliveryStatusOption.Cancelled => "error",
        _ => "default"
    };

    public static string GetStatusLabel(DeliveryStatusOption status) => status switch
    {
        DeliveryStatusOption.Pending => "Pending",
        DeliveryStatusOption.OnTheWay => "On The Way",
        DeliveryStatusOption.Delivered => "Delivered",
        DeliveryStatusOption.Cancelled => "Cancelled",
        _ => "Unknown"
    };

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

    public enum DeliveryStatusOption
    {
        Pending,
        OnTheWay,
        Delivered,
        Cancelled
    }
    public enum SaleSearchCategory
    {
        ReceiptNumber,
        ClientName
    }

}
