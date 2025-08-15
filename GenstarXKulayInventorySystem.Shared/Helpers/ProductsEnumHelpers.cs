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
        CubicMeter

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
        None
    }
}
