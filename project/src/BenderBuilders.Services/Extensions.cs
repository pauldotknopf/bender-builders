using System.Data;
using BenderBuilders.Interfaces;
using BenderBuilders.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace BenderBuilders.Services;

public static class Extensions
{
    public static IServiceCollection AddBenderBuildersServices(this IServiceCollection services)
    {
        services.AddSingleton<SharpDataAccess.Migrations.IMigrator, SharpDataAccess.Migrations.Impl.Migrator>();
        services.AddSingleton<SharpDataAccess.Data.IDataService, SharpDataAccess.Data.Impl.DataService>();
        services.AddSingleton<SharpDataAccess.IDataAccessLogger, SharpDataAccess.Impl.ConsoleDataAccessLogger>();
        services.AddSingleton(new SharpDataAccess.Data.DataOptions
        {
            DefaultTransactionIsolationLevel = IsolationLevel.Serializable
        });
        services.AddSingleton<SharpDataAccess.Migrations.IMigrationsBuilder, MigrationsBuilder>();
        services.AddSingleton<SharpDataAccess.Data.IDbConnectionFactoryProvider, SqliteHomeDirFactoryProvider>();
        
        services.AddSingleton<IProposalService, ProposalService>();
        services.AddSingleton<IInvoiceService, InvoiceService>();
        services.AddSingleton<IInvoiceLineItemService, InvoiceLineItemService>();

        return services;
    }
}