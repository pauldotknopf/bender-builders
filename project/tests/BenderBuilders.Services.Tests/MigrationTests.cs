using BenderBuilders.Services.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class MigrationTests : DataTestBase
{
    [TestMethod]
    public async Task Can_run_migrations()
    {
        var sp = BuildServiceProvider();
        var migrator = sp.GetRequiredService<IMigrator>();
        var dataService = sp.GetRequiredService<IDataService>();
        
        migrator.Migrate();

        using (var conScope = new ConScope(dataService))
        {
            var connection = conScope.Connection;

            var proposal = new Proposal();
            proposal.CustomerName = "John Doe";
            connection.Save(proposal);
        }
        
        using (var conScope = new ConScope(dataService))
        {
            var connection = conScope.Connection;

            var proposals = connection.Select<Proposal>();
            proposals.Should().HaveCount(1);
            proposals.First().CustomerName.Should().Be("John Doe");
        }
    }
}