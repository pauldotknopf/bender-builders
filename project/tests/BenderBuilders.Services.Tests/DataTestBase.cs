using BenderBuilders.Services.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using SharpDataAccess.Data;

namespace BenderBuilders.Services.Tests;

public class DataTestBase : TestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddBenderBuildersServices();
        services.AddSingleton<IDbConnectionFactoryProvider, SqliteMemoryFactoryProvider>();
        base.ConfigureServices(services);
    }
}