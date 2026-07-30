using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using BenderBuilders.Services.Models;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Impl;

public class ProposalService : IProposalService
{
    private readonly IMigrator _migrator;
    private readonly IDataService _dataService;

    public ProposalService(IMigrator migrator, IDataService dataService)
    {
        _migrator = migrator;
        _dataService = dataService;
    }

    public async Task<IReadOnlyList<ProposalDto>> GetRecentProposalsAsync(int count)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var query = conScope.Connection.From<Proposal>()
            .OrderByDescending(p => p.ProposalDate)
            .ThenByDescending(p => p.Id)
            .Limit(count);

        var proposals = await conScope.Connection.SelectAsync(query);

        return proposals.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ProposalDto>> GetAllProposalsAsync()
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var query = conScope.Connection.From<Proposal>()
            .OrderByDescending(p => p.ProposalDate)
            .ThenByDescending(p => p.Id);

        var proposals = await conScope.Connection.SelectAsync(query);

        return proposals.Select(MapToDto).ToList();
    }

    public async Task<ProposalDto?> GetProposalAsync(int id)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var proposal = await conScope.Connection.SingleByIdAsync<Proposal>(id);

        return proposal is null ? null : MapToDto(proposal);
    }

    public async Task<ProposalDto> SaveProposalAsync(ProposalDto proposal)
    {
        _migrator.Migrate();

        using var conScope = new ConScope(await ConScope.GetAsyncContext(_dataService));

        var model = MapToModel(proposal);

        await conScope.Connection.SaveAsync(model);

        return MapToDto(model);
    }

    private static ProposalDto MapToDto(Proposal p) => new()
    {
        Id = p.Id,
        CustomerName = p.CustomerName,
        ProposalDate = p.ProposalDate,
        Address1 = p.Address1,
        Address2 = p.Address2,
        City = p.City,
        State = p.State,
        PhoneNumber = p.PhoneNumber,
        JobLocation = p.JobLocation,
        FedIdNumber = p.FedIdNumber,
        ProposalSummary = p.ProposalSummary
    };

    private static Proposal MapToModel(ProposalDto p) => new()
    {
        Id = p.Id,
        CustomerName = p.CustomerName,
        ProposalDate = p.ProposalDate,
        Address1 = p.Address1,
        Address2 = p.Address2,
        City = p.City,
        State = p.State,
        PhoneNumber = p.PhoneNumber,
        JobLocation = p.JobLocation,
        FedIdNumber = p.FedIdNumber,
        ProposalSummary = p.ProposalSummary
    };
}
