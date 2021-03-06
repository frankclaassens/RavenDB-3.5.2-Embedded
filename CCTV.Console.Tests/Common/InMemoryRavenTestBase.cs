﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using CCTV.Entities;
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
    public class InMemoryRavenTestBase : RavenTestBase
    {
        private static readonly string DataDirRoot = ConfigurationManager.AppSettings["EmbeddedRavenPath"];

        private static EmbeddableDocumentStore _store;

        private static bool _startRavenStudio;

        protected bool StartRavenStudio
        {
            get { return _startRavenStudio; }
            set { _startRavenStudio = value; }
        }

        protected static Task _seedTask;

        protected IDocumentStore Store => _store;


        public InMemoryRavenTestBase()
        {
            TimeAndExecute("Creating Raven Store", () => _store = CreateStore());

            Task.Run(() => CreateDocumentStoreIndexes());

            //Console.WriteLine("Async Task => Launch RavenDB Management Studio");
            //Task.Run(() => WaitForUserToContinueTheTest(false, "http://localhost:8080"));

            _seedTask = Task.Run(() => TimeAndExecute("Seeding Database", GenerateSeedData));

            //WaitForIndexing(_store);
        }

        private EmbeddableDocumentStore CreateStore()
        {
            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);

            _store = _store = NewDocumentStore(
                true,
                requestedStorage: "esent",
                port: 8080,
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

            store.Configuration.PluginsDirectory = @"~\Plugins";
            store.Configuration.CreatePluginsDirectoryIfNotExisting = true;
            store.Configuration.AllowLocalAccessWithoutAuthorization = true;
            store.Configuration.AnonymousUserAccessMode = AnonymousUserAccessMode.Admin;
            store.Configuration.AccessControlAllowOrigin = new HashSet<string>() { "*" };
            store.Configuration.NewIndexInMemoryMaxBytes = 1073741824;

            store.Conventions = new DocumentConvention()
            {
                DisableProfiling = true,
                MaxNumberOfRequestsPerSession = 30,
                DefaultQueryingConsistency = ConsistencyOptions.None
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
        }

        private static void CreateDocumentStoreIndexes()
        {
            IndexCreation.CreateIndexes(typeof(AutoEmployeesByFirstNameIndex).Assembly, _store);
        }

        private void GenerateSeedData()
        {
            var employees = new Faker<Employee>()
             .RuleFor(u => u.FirstName, f => $"Name {f.UniqueIndex}")
             .RuleFor(u => u.LastName, f => f.Name.LastName())
             .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            // .RuleFor(u => u.HomePhone, (f, u) => f.Phone.PhoneNumber())
             .RuleFor(u => u.Description, (f, u) => f.Random.AlphaNumeric(5000))
             .Generate(20000).ToArray();

            using (var session = Store.OpenSession())
            {
                employees.ForEach(e => session.Store(e));
                session.SaveChanges();
                //foreach (var employee in employees)
                //{
                //    session.Store(employee);
                //    session.SaveChanges();
                //}                
            }
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
