using ServiceStack.DataAnnotations;

namespace BenderBuilders.Services.Models;

/// <summary>
/// ORM-mapped model representing a single line item on an invoice.
/// </summary>
[Alias("InvoiceLineItem")]
public class InvoiceLineItem
{
    [AutoIncrement]
    public int Id { get; set; }

    [References(typeof(Invoice))]
    public int InvoiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Line item amount. May be negative (e.g. discounts / adjustments).
    /// </summary>
    public decimal Amount { get; set; }
}
