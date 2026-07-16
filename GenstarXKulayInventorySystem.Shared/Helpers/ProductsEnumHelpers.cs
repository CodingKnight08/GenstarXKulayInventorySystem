namespace GenstarXKulayInventorySystem.Shared.Helpers;

public class ProductsEnumHelpers
{
    public enum BranchOption
    {
        Polomolok,
        GeneralSantosCity,
        Warehouse,
    }

    public enum  ProductMesurementOption
    {
        Milliliter,
        FluidOunce,
        Liter,
        Quart,
        Pint,
        Gallon,
        Bottle,
        Can,
        Sachet,

        Gram,
        Kilogram,

        Millimeter,
        Centimeter,
        Meter,
        Yard,
        Feet,

        Piece,
        Dozen,
        Pack,
        Bag,
        Roll,
        Box,
        Pallet,
        Sheet,

        SquareMeter,
        CubicMeter,
        Pail,
        Set,
        Sack,
        Inch

    }

    public enum ProductPricingOption
    {
        Retail,
        WholeSale,
        Override
    }

    public enum PaintCategory
    {
        Combo,
        Mix,
        None,
        Solid,
        Repack
    }

    public static string ProductUnit(ProductMesurementOption unit)
    {
        return unit switch
        {
            ProductMesurementOption.Milliliter => "(ML)",
            ProductMesurementOption.FluidOunce => "(FLOZ)",
            ProductMesurementOption.Liter => "(L)",
            ProductMesurementOption.Quart => "(QRT)",
            ProductMesurementOption.Pint => "(PINT)",
            ProductMesurementOption.Gallon => "(GAL)",
            ProductMesurementOption.Bottle => "(BOT)",
            ProductMesurementOption.Can => "(CAN)",
            ProductMesurementOption.Sachet => "(SACH)",

            ProductMesurementOption.Gram => "(GMS)",
            ProductMesurementOption.Kilogram => "(KLS)",

            ProductMesurementOption.Millimeter => "(MM)",
            ProductMesurementOption.Centimeter => "(CM)",
            ProductMesurementOption.Meter => "(M)",
            ProductMesurementOption.Yard => "(YRD)",
            ProductMesurementOption.Feet => "(FT)",
            ProductMesurementOption.Inch => "(IN)",

            ProductMesurementOption.Piece => "(PC)",
            ProductMesurementOption.Dozen => "(DOZ)",
            ProductMesurementOption.Pack => "(PCK)",
            ProductMesurementOption.Bag => "(BAG)",
            ProductMesurementOption.Roll => "(ROLL)",
            ProductMesurementOption.Box => "(BOX)",
            ProductMesurementOption.Pallet => "(PLT)",
            ProductMesurementOption.Sheet => "(SHEET)",

            ProductMesurementOption.SquareMeter => "(M2)",
            ProductMesurementOption.CubicMeter => "(CM2)",
            ProductMesurementOption.Pail => "(PAIL)",
            ProductMesurementOption.Set => "(SET)",
            ProductMesurementOption.Sack => "(SCK)",

            _ => unit.ToString()
        };
    }
}
