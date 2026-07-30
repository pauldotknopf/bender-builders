using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;

namespace BenderBuilders.Services.Impl;

public class SqliteHomeDirFactoryProvider : IDbConnectionFactoryProvider
{
    public IDbConnectionFactory BuildConnectionFactory()
    {
        var sqliteDialect = SqliteDialect.Create();
        SqliteConfiguration.Configure(sqliteDialect);
        var homeDir = SpecialDirectories.CurrentUserApplicationData;
        return new OrmLiteConnectionFactory($"Data Source={Path.Combine(homeDir, "Application.Db")};Cache=Shared", sqliteDialect);
    }
}