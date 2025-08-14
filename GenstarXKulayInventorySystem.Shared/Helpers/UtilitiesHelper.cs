namespace GenstarXKulayInventorySystem.Shared.Helpers;

public static class UtilitiesHelper
{
    public static DateTime GetPhilippineTime()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }

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
