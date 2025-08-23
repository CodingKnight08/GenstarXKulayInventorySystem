using GenstarXKulayInventorySystem.Shared.DTOS;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.Helpers;

public static class UtilitiesHelper
{
    public static DateTime GetPhilippineTime()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
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
