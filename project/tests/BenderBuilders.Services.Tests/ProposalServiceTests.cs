using BenderBuilders.Interfaces;
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
}
