using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents inventory and production items.
/// </summary>
public class Product : BaseEntity, ISoftDeletable
{
    public Guid IdProduct { get; set; }

    /// <summary>
    /// Reference to the vendor supplying this product.
    /// </summary>
    public Guid? IdVendor { get; set; }

    public string? ProductName { get; set; }
    public byte[]? ProductImage { get; set; }

    public decimal? QuantityPerUnit { get; set; }
    public string? UnitDimension { get; set; }

    public decimal? UnitsInStock { get; set; }
    public decimal? UnitsInProduction { get; set; }
    public decimal? UnitsAtCustomer { get; set; }

    /// <summary>
    /// Time units per production (TeH = Time per unit in hours/minutes).
    /// </summary>
    public decimal? TeHMin { get; set; }

    /// <summary>
    /// Indicates if this is a returning product service.
    /// </summary>
    public bool? ReturningProductService { get; set; }

    public decimal? UnitMainPrice { get; set; }
    public decimal? VAT { get; set; }
    public bool Discontinued { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual Vendor? Vendor { get; set; }
}
