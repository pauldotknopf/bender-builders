using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.Interfaces;

public interface IInvoiceService
{
    /// <summary>
    /// Returns all invoices belonging to the given proposal, ordered by invoice date descending.
    /// </summary>
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesForProposalAsync(int proposalId);

    /// <summary>
    /// Returns the invoice with the given id, or <c>null</c> if none exists.
    /// </summary>
    Task<InvoiceDto?> GetInvoiceAsync(int id);

    /// <summary>
    /// Creates or updates an invoice. When <see cref="InvoiceDto.Id"/> is 0 a new invoice is
    /// inserted; otherwise the existing invoice is updated. Returns the saved invoice with its
    /// <see cref="InvoiceDto.Id"/> populated. Line items are managed separately via
    /// <see cref="IInvoiceLineItemService"/>.
    /// </summary>
    Task<InvoiceDto> SaveInvoiceAsync(InvoiceDto invoice);

    /// <summary>
    /// Deletes the invoice with the given id along with all of its line items. Returns
    /// <c>true</c> when an invoice was deleted, <c>false</c> when no such invoice existed.
    /// </summary>
    Task<bool> DeleteInvoiceAsync(int id);
}
