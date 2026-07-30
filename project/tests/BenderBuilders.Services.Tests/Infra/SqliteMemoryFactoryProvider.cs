using ServiceStack.Data;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;

namespace BenderBuilders.Services.Tests.Infra;

public class SqliteMemoryFactoryProvider : IDbConnectionFactoryProvider
{
    public IDbConnectionFactory BuildConnectionFactory()
    {
        var sqliteDialect = SqliteDialect.Create();
        SqliteConfiguration.Configure(sqliteDialect);
        return new OrmLiteConnectionFactory(":memory:", sqliteDialect);
    }
}