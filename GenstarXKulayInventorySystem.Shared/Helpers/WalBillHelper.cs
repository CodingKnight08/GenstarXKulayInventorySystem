namespace GenstarXKulayInventorySystem.Shared.Helpers;

public class WalBillHelper
{
    public enum WayBillTermsOption
    {
        Day7 = 7,
        Day15 = 15,
        Day30 = 30,
        Day60 = 60,
        Day90 = 90,
        Day120 = 120
    }

    public static string GetWayBillTermsText(WayBillTermsOption option)
    {
        switch (option)
        {
            case WayBillTermsOption.Day7:
                return "7 Days";

            case WayBillTermsOption.Day15:
                return "15 Days";

            case WayBillTermsOption.Day30:
                return "30 Days";

            case WayBillTermsOption.Day60:
                return "60 Days";

            case WayBillTermsOption.Day90:
                return "90 Days";

            case WayBillTermsOption.Day120:
                return "120 Days";

            default:
                return string.Empty;
        }
    }
}
