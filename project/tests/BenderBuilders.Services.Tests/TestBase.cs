using System.Security.Authentication.ExtendedProtection;
using Microsoft.Extensions.DependencyInjection;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class TestBase
{
    protected IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        
    }
}