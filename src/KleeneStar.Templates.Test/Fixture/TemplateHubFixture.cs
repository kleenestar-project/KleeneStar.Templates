using KleeneStar.Automation;
using KleeneStar.Core;
using KleeneStar.Model;
using KleeneStar.Model.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Reflection;
using WebExpress.WebCore;
using WebExpress.WebCore.WebComponent;

namespace KleeneStar.Templates.Test.Fixture
{
    /// <summary>
    /// Wires the core and the automation plugin to an isolated in-memory database: every manager
    /// backing field of <see cref="CoreHub"/> and <see cref="AutomationHub"/> gets a fresh
    /// instance, found by reflection, so the fixture never has to be kept in step with the hubs.
    /// </summary>
    internal static class TemplateHubFixture
    {
        /// <summary>
        /// Points the model at an in-memory database and builds every manager.
        /// </summary>
        /// <param name="database">A unique name of the in-memory database.</param>
        public static void Initialize(string database)
        {
            // ModelHub is internal to the model; its settings are reached by reflection
            typeof(DatabaseSettings).Assembly.GetType("KleeneStar.Model.ModelHub", throwOnError: true)
                .GetProperty("DatabaseSettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, new DatabaseSettings
                {
                    Assembly = typeof(TemplateHubFixture).Assembly.GetName().Name,
                    ConnectionString = database
                });

            foreach (var hub in new[] { typeof(CoreHub), typeof(AutomationHub) })
            {
                foreach (var field in hub.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var ctor = field.FieldType.IsClass && !field.FieldType.IsAbstract && typeof(IComponentManager).IsAssignableFrom(field.FieldType)
                        ? field.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(IComponentHub), typeof(IHttpServerContext)], null)
                        : null;

                    if (ctor is not null)
                    {
                        field.SetValue(null, ctor.Invoke([null, null]));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a context over the same in-memory database, for the arrange phase.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>The context.</returns>
        public static KleeneStarDbContext CreateDbContext(string database)
        {
            return InMemoryDbContextFactory.Create(database);
        }
    }

    /// <summary>
    /// The provider the model loads by the fixture's assembly name: the first public static
    /// <c>Create(string)</c> of this assembly.
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        private static readonly ConcurrentDictionary<string, DbContextOptions<KleeneStarDbContext>> _options = new();

        /// <summary>
        /// Creates a context over the named in-memory database.
        /// </summary>
        /// <param name="connectionString">The database name.</param>
        /// <returns>The context.</returns>
        public static KleeneStarDbContext Create(string connectionString)
        {
            var key = string.IsNullOrWhiteSpace(connectionString) ? "TemplatesTestDb" : connectionString;
            var options = _options.GetOrAdd(key, name => new DbContextOptionsBuilder<KleeneStarDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            return new KleeneStarDbContext(options);
        }
    }
}
