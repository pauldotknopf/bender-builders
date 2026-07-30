using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using BenderBuilders.Services.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class ProposalServiceTests : DataTestBase
{
    [TestMethod]
    public async Task GetRecentProposals_returns_most_recent_first_and_respects_count()
    {
        var sp = BuildServiceProvider();
        var dataService = sp.GetRequiredService<IDataService>();
        var proposalService = sp.GetRequiredService<IProposalService>();

        // The service migrates on-demand, so seed after it has run once.
        await proposalService.GetRecentProposalsAsync(1);
        
        using (var conScope = new ConScope(dataService))
        {
            var connection = conScope.Connection;
            connection.Save(new Proposal { CustomerName = "Oldest", ProposalDate = new DateTime(2026, 1, 1) });
            connection.Save(new Proposal { CustomerName = "Middle", ProposalDate = new DateTime(2026, 6, 15) });
            connection.Save(new Proposal { CustomerName = "Newest", ProposalDate = new DateTime(2026, 12, 31) });
        }

        var recent = await proposalService.GetRecentProposalsAsync(2);

        recent.Should().HaveCount(2);
        recent[0].CustomerName.Should().Be("Newest");
        recent[1].CustomerName.Should().Be("Middle");
    }

    [TestMethod]
    public async Task GetRecentProposals_returns_empty_when_no_proposals()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var recent = await proposalService.GetRecentProposalsAsync(5);

        recent.Should().BeEmpty();
    }

    [TestMethod]
    public async Task SaveProposalAsync_inserts_a_new_proposal()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var saved = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = "Jane Doe",
            ProposalDate = new DateTime(2026, 5, 1),
            City = "Springfield"
        });

        saved.Id.Should().BeGreaterThan(0);

        var fetched = await proposalService.GetProposalAsync(saved.Id);
        fetched.Should().NotBeNull();
        fetched!.CustomerName.Should().Be("Jane Doe");
        fetched.City.Should().Be("Springfield");
        fetched.ProposalDate.Should().Be(new DateTime(2026, 5, 1));
    }

    [TestMethod]
    public async Task SaveProposalAsync_updates_an_existing_proposal()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var saved = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = "Original",
            ProposalDate = new DateTime(2026, 5, 1)
        });

        saved.CustomerName = "Updated";
        var updated = await proposalService.SaveProposalAsync(saved);

        updated.Id.Should().Be(saved.Id);

        var all = await proposalService.GetAllProposalsAsync();
        all.Should().HaveCount(1);
        all[0].CustomerName.Should().Be("Updated");
    }

    [TestMethod]
    public async Task GetProposalAsync_returns_null_when_missing()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var fetched = await proposalService.GetProposalAsync(999);

        fetched.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAllProposalsAsync_returns_all_ordered_by_date_descending()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Oldest", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Newest", ProposalDate = new DateTime(2026, 12, 31) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Middle", ProposalDate = new DateTime(2026, 6, 15) });

        var all = await proposalService.GetAllProposalsAsync();

        all.Should().HaveCount(3);
        all.Select(p => p.CustomerName).Should().ContainInOrder("Newest", "Middle", "Oldest");
    }
}
