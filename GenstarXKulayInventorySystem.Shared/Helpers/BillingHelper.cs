using System.ComponentModel;

namespace GenstarXKulayInventorySystem.Shared.Helpers;

public class BillingHelper
{
    public enum BillingCategory
    {
        Logistics,
        Electric,
        Internet,
        Telephone,
        Water,
        Cellphone,
        SchoolSupplies,
        Other,
        Foods,
    }

    public enum BillingBranch
    {
        GenStar,
        Kulay,
        Warehouse,
    }

    public enum PaymentTermsOption
    {
        [Description("Today")]
        Today = 0,

        [Description("7 Days")]
        SevenDays = 7,

        [Description("30 Days")]
        ThirtyDays = 30,

        [Description("60 Days")]
        SixtyDays = 60,

        [Description("90 Days")]
        NinetyDays = 90,
          
        [Description("Custom")]
         Custom = -1
    }
}
