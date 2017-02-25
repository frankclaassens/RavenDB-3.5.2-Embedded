using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTV.Console.Tests
{
    using System.ComponentModel.Composition.Hosting;
    using System.Configuration;
    using System.IO;
    using Raven.Abstractions.Data;
    using Raven.Bundles.UniqueConstraints;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Embedded;
    using Raven.Client.Indexes;
    using Raven.Client.UniqueConstraints;
    using Raven.Database.Config;
    using Raven.Database.Server;
    using Raven.Tests.Helpers;
    using RavenDB.Indexes;

    public class CustomInMemoryRavenTestBase : RavenTestBase
    {
        private static readonly string DataDirRoot = ConfigurationManager.AppSettings["EmbeddedRavenDbPath"];

        private static EmbeddableDocumentStore _store;

        private static Task _initialiseStoreTask;

        protected IDocumentStore Store => _store;

        public CustomInMemoryRavenTestBase()
        {
            _store = CreateStore();
            _store.Initialize();

            InitIndexes();

            WaitForIndexing(_store);
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

            store.Conventions = new DocumentConvention()
            {
                DisableProfiling = true,
                MaxNumberOfRequestsPerSession = 30,
                DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite                
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
        }

        private static void InitIndexes()
        {
            IndexCreation.CreateIndexes(typeof(AutoEmployeesByFirstName).Assembly, _store);
        }
    }
}
