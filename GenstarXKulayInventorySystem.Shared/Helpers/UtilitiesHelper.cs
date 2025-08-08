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
}
