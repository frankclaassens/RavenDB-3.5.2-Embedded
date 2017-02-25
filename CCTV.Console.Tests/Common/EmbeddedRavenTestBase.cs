using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using CCTV.Domain.Entities;
using CCTV.RavenDB.Indexes;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using Raven.Bundles.UniqueConstraints;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Client.UniqueConstraints;
using Raven.Database.Server;
using Raven.Tests.Helpers;

namespace CCTV.Console.Tests.Common
{
    public class EmbeddedRavenTestBase : RavenTestBase
    {
        private static readonly string DataDirRoot = ConfigurationManager.AppSettings["EmbeddedRavenDbPath"];

        private static EmbeddableDocumentStore _store;

        protected static Task _seedTask;

        protected IDocumentStore Store => _store;

        public EmbeddedRavenTestBase()
        {
            TimeAndExecute("Creating Raven Store", () => _store = CreateStore());

            Task.Run(() => CreateDocumentStoreIndexes());
            //Task.Run(() => WaitForUserToContinueTheTest(true, "http://localhost:8080"));

            //_seedTask = Task.Run(() => GenerateSeedData());
            TimeAndExecute("Seeding Database", GenerateSeedData);
            //Task.Run(() => GenerateSeedData());


            //_initialiseStoreTask = Task.Run(() => CreateDocumentStoreIndexes());

            //WaitForIndexing(_store);

        }

        private EmbeddableDocumentStore CreateStore()
        {
            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);

            _store = _store = NewDocumentStore(
                false,
                dataDir: DataDirRoot,
                port: 8080,
                requestedStorage: "esent",
                activeBundles: "Unique Constraints;PeriodicExport;DocumentExpiration",
                catalog: new AggregateCatalog(new AssemblyCatalog(typeof(UniqueConstraintsPutTrigger).Assembly)),
                enableAuthentication: false,
                configureStore: ConfigureStore
            );

            return _store;
        }

        private void ConfigureStore(EmbeddableDocumentStore store)
        {
            store.UseEmbeddedHttpServer = true;

            store.DataDirectory = Path.Combine(DataDirRoot, Guid.NewGuid().ToString());
            store.Configuration.PluginsDirectory = @"~\Plugins";
            store.Configuration.CreatePluginsDirectoryIfNotExisting = true;
            store.Configuration.AllowLocalAccessWithoutAuthorization = true;
            store.Configuration.AnonymousUserAccessMode = AnonymousUserAccessMode.Admin;
            store.Configuration.AccessControlAllowOrigin = new HashSet<string>() { "*" };
            //store.Configuration.NewIndexInMemoryMaxBytes = 1073741824;
            store.Conventions = new DocumentConvention()
            {
                DisableProfiling = true,
                MaxNumberOfRequestsPerSession = 30,
                DefaultQueryingConsistency = ConsistencyOptions.None
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
        }

        private void GenerateSeedData()
        {
            var employees = new Faker<Employee>()
             .RuleFor(u => u.FirstName, f => $"Name {f.UniqueIndex}")
             .RuleFor(u => u.LastName, f => f.Name.LastName())
             .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
             .RuleFor(u => u.HomePhone, (f, u) => f.Phone.PhoneNumber())
             .RuleFor(u => u.Description, (f, u) => f.Random.AlphaNumeric(5000))
             .RuleFor(u => u.Birthday, (f, u) => f.Date.Past())
             .Generate(5000).ToArray();

            using (var session = Store.OpenSession())
            {
                employees.ForEach(e=> session.Store(e));
                session.SaveChanges();
                //foreach (var employee in employees)
                //{
                //    session.Store(employee);
                //    session.SaveChanges();
                //}                
            }
        }

        private static void CreateDocumentStoreIndexes()
        {
            IndexCreation.CreateIndexes(typeof(AutoEmployeesByFirstNameIndex).Assembly, _store);
        }

        private void TimeAndExecute(string message, Action task)
        {
            var sb = new StringBuilder();
            sb.Append(message.PadRight(40));

            var sw = new Stopwatch();
            sw.Start();
            task.Invoke();
            sw.Stop();

            sb.Append($"[{sw.Elapsed.Minutes:00}:{sw.Elapsed.Seconds:00}:{sw.Elapsed.Milliseconds:000}]");
            TestContext.WriteLine(sb.ToString());
        }
    }
}
