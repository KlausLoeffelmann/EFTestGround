namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a product for inventory and production tracking.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdProduct { get; set; }

    /// <summary>
    /// Vendor who supplies this product.
    /// </summary>
    public Guid? IdVendor { get; set; }

    /// <summary>
    /// Navigation property to the vendor.
    /// </summary>
    public Vendor? Vendor { get; set; }

    /// <summary>
    /// Name of the product.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Product image.
    /// </summary>
    public byte[]? ProductImage { get; set; }

    /// <summary>
    /// Quantity per unit.
    /// </summary>
    public decimal? QuantityPerUnit { get; set; }

    /// <summary>
    /// Unit dimension.
    /// </summary>
    public string? UnitDimension { get; set; }

    /// <summary>
    /// Units in stock.
    /// </summary>
    public decimal? UnitsInStock { get; set; }

    /// <summary>
    /// Units in production.
    /// </summary>
    public decimal? UnitsInProduction { get; set; }

    /// <summary>
    /// Units at customer location.
    /// </summary>
    public decimal? UnitsAtCustomer { get; set; }

    /// <summary>
    /// Time per unit in minutes.
    /// </summary>
    public decimal? TeHMin { get; set; }

    /// <summary>
    /// Indicates whether this is a returning product service.
    /// </summary>
    public bool? ReturningProductService { get; set; }

    /// <summary>
    /// Main unit price.
    /// </summary>
    public decimal? UnitMainPrice { get; set; }

    /// <summary>
    /// VAT percentage.
    /// </summary>
    public decimal? VAT { get; set; }

    /// <summary>
    /// Indicates whether this product is discontinued.
    /// </summary>
    public bool Discontinued { get; set; }
}