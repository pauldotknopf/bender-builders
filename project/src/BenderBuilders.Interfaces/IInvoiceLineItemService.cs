using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.Interfaces;

public interface IInvoiceLineItemService
{
    /// <summary>
    /// Returns all line items belonging to the given invoice, ordered by id ascending.
    /// </summary>
    Task<IReadOnlyList<InvoiceLineItemDto>> GetLineItemsForInvoiceAsync(int invoiceId);

    /// <summary>
    /// Returns the line item with the given id, or <c>null</c> if none exists.
    /// </summary>
    Task<InvoiceLineItemDto> GetLineItemAsync(int id);

    /// <summary>
    /// Creates or updates a single line item. When <see cref="InvoiceLineItemDto.Id"/> is 0 a new
    /// line item is inserted; otherwise the existing line item is updated. Returns the saved line
    /// item with its <see cref="InvoiceLineItemDto.Id"/> populated.
    /// </summary>
    Task<InvoiceLineItemDto> SaveLineItemAsync(InvoiceLineItemDto lineItem);

    /// <summary>
    /// Replaces the complete set of line items for an invoice with the supplied collection. Line
    /// items that currently exist on the invoice but are absent from <paramref name="lineItems"/>
    /// are deleted. This supports the create/update page where line items are managed client-side
    /// and posted back as a whole. Returns the saved line items with their ids populated.
    /// </summary>
    Task<IReadOnlyList<InvoiceLineItemDto>> ReplaceLineItemsForInvoiceAsync(int invoiceId, IReadOnlyList<InvoiceLineItemDto> lineItems);

    /// <summary>
    /// Deletes the line item with the given id. Returns <c>true</c> when a line item was deleted,
    /// <c>false</c> when no such line item existed.
    /// </summary>
    Task<bool> DeleteLineItemAsync(int id);
}
