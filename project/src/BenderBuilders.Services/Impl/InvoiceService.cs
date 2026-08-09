using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using BenderBuilders.Services.Models;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Impl;

public class InvoiceService : IInvoiceService
{
    private readonly IMigrator _migrator;
    private readonly IDataService _dataService;

    public InvoiceService(IMigrator migrator, IDataService dataService)
    {
        _migrator = migrator;
        _dataService = dataService;
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesForProposalAsync(int proposalId)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var query = conScope.Connection.From<Invoice>()
            .Where(i => i.ProposalId == proposalId)
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id);

        var invoices = await conScope.Connection.SelectAsync(query);

        return invoices.Select(MapToDto).ToList();
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(int id)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var invoice = await conScope.Connection.SingleByIdAsync<Invoice>(id);

        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<InvoiceDto> SaveInvoiceAsync(InvoiceDto invoice)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var model = MapToModel(invoice);

        await conScope.Connection.SaveAsync(model);

        return MapToDto(model);
    }

    public async Task<bool> DeleteInvoiceAsync(int id)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));
        using var transScope = await conScope.BeginTransaction();

        var invoice = await conScope.Connection.SingleByIdAsync<Invoice>(id);
        if (invoice is null)
        {
            return false;
        }

        await conScope.Connection.DeleteAsync<InvoiceLineItem>(x => x.InvoiceId == id);
        await conScope.Connection.DeleteByIdAsync<Invoice>(id);

        transScope.Commit();

        return true;
    }

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        Id = i.Id,
        ProposalId = i.ProposalId,
        InvoiceDate = i.InvoiceDate
    };

    private static Invoice MapToModel(InvoiceDto i) => new()
    {
        Id = i.Id,
        ProposalId = i.ProposalId,
        InvoiceDate = i.InvoiceDate
    };
}
