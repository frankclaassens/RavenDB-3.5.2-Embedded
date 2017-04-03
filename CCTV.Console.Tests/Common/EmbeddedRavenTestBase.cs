using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using CCTV.Entities;
using CCTV.RavenDB.Indexes;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Bundles.UniqueConstraints;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using Raven.Client.UniqueConstraints;
using Raven.Database;
using Raven.Database.Server;
using Raven.Tests.Helpers;

namespace CCTV.Console.Tests.Common
{
    public class EmbeddedRavenTestBase : RavenTestBase
    {
        private static readonly string DatabaseGuid = Guid.NewGuid().ToString();

        private string EmbeddedDatabasePath => Path.Combine(ConfigurationManager.AppSettings["EmbeddedRavenPath"], DatabaseGuid);

        private bool createSeededTestDatabase = true;

        private static EmbeddableDocumentStore _store;

        protected IDocumentStore Store => _store;

        protected EmbeddedRavenTestBase()
        {
            // Create an unique folder name for each new database
            _store = CreateStore();

            // CreateDatabaseResource("StudioHub");

            CreateDocumentStoreIndexes();
            GenerateSeedData();

            //TestContext.WriteLine("BACKUP STARTED");
            //var task = BackupDatabaseAsync();
            //task.Wait();
            //TestContext.WriteLine("BACKUP ENDED");

            //TestContext.WriteLine("RESTORE STARTED");
            //var restoreTask = RestoreDatabaseAsync();
            //restoreTask.Wait();
            //TestContext.WriteLine("RESTORE ENDED");
        }

        private EmbeddableDocumentStore CreateStore()
        {
            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8079);

            _store = NewDocumentStore(
                false,
                port: 8079,
                requestedStorage: "esent",
                activeBundles: "Unique Constraints",
                catalog: new AggregateCatalog(new AssemblyCatalog(typeof(UniqueConstraintsPutTrigger).Assembly)),
                enableAuthentication: false,
                configureStore: ConfigureStore
            );

            return _store;
        }

        private void ConfigureStore(EmbeddableDocumentStore store)
        {
            if (createSeededTestDatabase)
            {
                store.DefaultDatabase = "StudioHub";

                store.DataDirectory = Path.Combine(EmbeddedDatabasePath, @"Databases\System");
                store.Configuration.PluginsDirectory = ConfigurePlugins();
                store.Configuration.CompiledIndexCacheDirectory = Path.Combine(EmbeddedDatabasePath, "CompiledIndexCache");
                store.Configuration.AccessControlAllowOrigin = new HashSet<string>() { "*" };

                store.Conventions = new DocumentConvention()
                {
                    DisableProfiling = true,
                    MaxNumberOfRequestsPerSession = 30,
                    DefaultQueryingConsistency = ConsistencyOptions.None
                };

                store.RegisterListener(new UniqueConstraintsStoreListener());
            }
            else
            {
                store.UseEmbeddedHttpServer = true;
                store.DefaultDatabase = "StudioHub";

                store.Configuration.RunInUnreliableYetFastModeThatIsNotSuitableForProduction = false;

                store.DataDirectory = Path.Combine(EmbeddedDatabasePath, @"Databases\");
                store.Configuration.PluginsDirectory = ConfigurePlugins();
                store.Configuration.CompiledIndexCacheDirectory = Path.Combine(EmbeddedDatabasePath, "CompiledIndexCache");
                store.Configuration.AccessControlAllowOrigin = new HashSet<string>() { "*" };

                store.Conventions = new DocumentConvention()
                {
                    DisableProfiling = true,
                    MaxNumberOfRequestsPerSession = 30,
                    DefaultQueryingConsistency = ConsistencyOptions.None
                };

                store.RegisterListener(new UniqueConstraintsStoreListener());
            }
            //store.UseEmbeddedHttpServer = true;

            //store.Configuration.RunInUnreliableYetFastModeThatIsNotSuitableForProduction = false;

            //store.DataDirectory = Path.Combine(EmbeddedDatabasePath, @"Databases\System");
            //store.Configuration.PluginsDirectory = ConfigurePlugins();
            //store.Configuration.CompiledIndexCacheDirectory = Path.Combine(EmbeddedDatabasePath, "CompiledIndexCache");
            //store.Configuration.AccessControlAllowOrigin = new HashSet<string>() { "*" };

            //store.Conventions = new DocumentConvention()
            //{
            //    DisableProfiling = true,
            //    MaxNumberOfRequestsPerSession = 30,
            //    DefaultQueryingConsistency = ConsistencyOptions.None
            //};

            //store.RegisterListener(new UniqueConstraintsStoreListener());
        }

        //private void CreateDatabaseResource(string databaseName)
        //{
        //    _store.DefaultDatabase = databaseName;
        //    _store
        //        .DatabaseCommands
        //        .GlobalAdmin
        //        .CreateDatabase(new DatabaseDocument()
        //        {
        //            Id = databaseName,
        //            Settings =
        //            {
        //                {"Raven/ActiveBundles", "Unique Constraints"},
        //                {"Raven/DataDir", Path.Combine(EmbeddedDatabasePath, $@"Databases\{databaseName}")}
        //            }
        //        });
        //}

        protected async Task LaunchRavenStudioGui()
        {
            var ravenStudioTask = Task.Run(() => WaitForUserToContinueTheTest(false, "http://localhost:8079")).ConfigureAwait(false);

            await ravenStudioTask;
        }

        private string ConfigurePlugins()
        {
            // Make sure Plugins folder exist and contains all bundle assemblies that are required by the embedded raven db.
            var outputDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var pluginDir = Path.Combine(outputDir, @"Plugins");

            if (!Directory.Exists(pluginDir)) Directory.CreateDirectory(pluginDir);

            var di = new DirectoryInfo(outputDir);
            var ravenBundles = di.GetFiles("Raven.Bundles.*.dll");
            foreach (var bundleFile in ravenBundles)
            {
                var destFile = Path.Combine(pluginDir, bundleFile.Name);
                // If bundle file does not exist in Plugin folder, copy it.
                if (!File.Exists(destFile))
                    File.Copy(bundleFile.FullName, destFile, true);

                // If bundle file exists, only copy if assembly version is newer
                if (FileVersionInfo.GetVersionInfo(bundleFile.FullName).FileBuildPart > FileVersionInfo.GetVersionInfo(destFile).FileBuildPart)
                    File.Copy(bundleFile.FullName, destFile, true);

            }

            return pluginDir;
        }

        private void GenerateSeedData()
        {
            var employees = new Faker<Employee>()
             .RuleFor(u => u.FirstName, f => $"Name {f.UniqueIndex}")
             .RuleFor(u => u.LastName, f => f.Name.LastName())
             .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
             .RuleFor(u => u.Description, (f, u) => f.Random.AlphaNumeric(50))
             .Generate(5000).ToArray();

            using (var session = Store.OpenSession())
            {
                employees.ForEach(e => session.Store(e));
                session.SaveChanges();
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

        public async Task BackupDatabaseAsync()
        {
            var backupDir = Path.Combine(@"C:\EmbeddedBackup\" + DatabaseGuid);
            Directory.CreateDirectory(backupDir);
           
            var backupOperation = Store.DatabaseCommands.GlobalAdmin.StartBackup(backupDir, null, false, "StudioHub");
            await backupOperation.WaitForCompletionAsync().ConfigureAwait(false);
        }

        protected async Task RestoreDatabaseAsync()
        {
            var backupDir = Path.Combine(@"C:\EmbeddedBackup\" + DatabaseGuid);

            if (!Directory.Exists(backupDir))
                throw new DirectoryNotFoundException("Backup folder not found");

            var restoreRequest = new DatabaseRestoreRequest
            {
                BackupLocation = backupDir,
                DatabaseLocation = Path.Combine(EmbeddedDatabasePath, @"Databases\StudioHub"),
                DatabaseName = "StudioHub"
            };

            Store.DatabaseCommands.GlobalAdmin.DeleteDatabase("StudioHub", true);

            var restoreOperation = Store.DatabaseCommands.GlobalAdmin.StartRestore(restoreRequest);
            await restoreOperation.WaitForCompletionAsync().ConfigureAwait(false);
        }
    }
}
