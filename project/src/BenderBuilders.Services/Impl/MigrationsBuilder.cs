using System.Reflection;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Impl;

public class MigrationsBuilder : IMigrationsBuilder
{
    public void BuildMigrations(Action<IList<IMigration>> action)
    {
        var migrations = new List<IMigration>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (typeof(IMigration).IsAssignableFrom(type))
            {
                migrations.Add((IMigration)Activator.CreateInstance(type));
            }
        }
        action(migrations);
    }
}
