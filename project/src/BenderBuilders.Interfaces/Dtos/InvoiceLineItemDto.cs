namespace BenderBuilders.Interfaces.Dtos;

/// <summary>
/// POCO data-transfer object representing a single line item on an invoice.
/// Maps to the <c>InvoiceLineItem</c> ORM model. The owning invoice is
/// referenced ID-based via <see cref="InvoiceId"/>.
/// </summary>
public class InvoiceLineItemDto
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Line item amount. May be negative (e.g. discounts / adjustments).
    /// </summary>
    public decimal Amount { get; set; }
}
