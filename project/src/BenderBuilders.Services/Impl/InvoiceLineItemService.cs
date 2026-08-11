using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using BenderBuilders.Services.Models;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Impl;

public class InvoiceLineItemService : IInvoiceLineItemService
{
    private readonly IMigrator _migrator;
    private readonly IDataService _dataService;

    public InvoiceLineItemService(IMigrator migrator, IDataService dataService)
    {
        _migrator = migrator;
        _dataService = dataService;
    }

    public async Task<IReadOnlyList<InvoiceLineItemDto>> GetLineItemsForInvoiceAsync(int invoiceId)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var lineItems = await SelectLineItemsAsync(conScope, invoiceId);

        return lineItems.Select(MapToDto).ToList();
    }

    public async Task<InvoiceLineItemDto> GetLineItemAsync(int id)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var lineItem = await conScope.Connection.SingleByIdAsync<InvoiceLineItem>(id);

        return lineItem is null ? null : MapToDto(lineItem);
    }

    public async Task<InvoiceLineItemDto> SaveLineItemAsync(InvoiceLineItemDto lineItem)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var model = MapToModel(lineItem);

        await conScope.Connection.SaveAsync(model);

        return MapToDto(model);
    }

    public async Task<IReadOnlyList<InvoiceLineItemDto>> ReplaceLineItemsForInvoiceAsync(int invoiceId, IReadOnlyList<InvoiceLineItemDto> lineItems)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));
        using var transScope = await conScope.BeginTransaction();

        var existingIds = (await SelectLineItemsAsync(conScope, invoiceId))
            .Select(x => x.Id)
            .ToHashSet();

        var keptIds = lineItems.Select(x => x.Id).Where(x => x != 0).ToHashSet();

        foreach (var removedId in existingIds.Where(x => !keptIds.Contains(x)))
        {
            await conScope.Connection.DeleteByIdAsync<InvoiceLineItem>(removedId);
        }

        var saved = new List<InvoiceLineItemDto>();

        foreach (var lineItem in lineItems)
        {
            // The invoice is the owner of the relationship, so ignore any mismatched InvoiceId
            // supplied by the caller. Ids that don't already belong to this invoice are treated
            // as inserts so a caller can't hijack another invoice's line item.
            var model = MapToModel(lineItem);
            model.InvoiceId = invoiceId;
            if (!existingIds.Contains(model.Id))
            {
                model.Id = 0;
            }

            await conScope.Connection.SaveAsync(model);

            saved.Add(MapToDto(model));
        }

        transScope.Commit();

        return saved;
    }

    public async Task<bool> DeleteLineItemAsync(int id)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var deleted = await conScope.Connection.DeleteByIdAsync<InvoiceLineItem>(id);

        return deleted > 0;
    }

    private static Task<List<InvoiceLineItem>> SelectLineItemsAsync(ConScope conScope, int invoiceId)
    {
        var query = conScope.Connection.From<InvoiceLineItem>()
            .Where(x => x.InvoiceId == invoiceId)
            .OrderBy(x => x.Id);

        return conScope.Connection.SelectAsync(query);
    }

    private static InvoiceLineItemDto MapToDto(InvoiceLineItem l) => new()
    {
        Id = l.Id,
        InvoiceId = l.InvoiceId,
        Description = l.Description,
        Amount = l.Amount
    };

    private static InvoiceLineItem MapToModel(InvoiceLineItemDto l) => new()
    {
        Id = l.Id,
        InvoiceId = l.InvoiceId,
        Description = l.Description,
        Amount = l.Amount
    };
}
