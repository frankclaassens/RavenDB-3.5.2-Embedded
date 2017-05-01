using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CCTV.RavenDB;
using NUnit.Framework;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class AdhocIndexTasks
    {
        private readonly IDocumentStore _store = DocumentStoreService.Store;

        [Test]
        public void RESET_USERINDEX_AND_WAIT_FOR_NON_STALE()
        {
            _store.DatabaseCommands.ResetIndex("UserIndex");
            WaitForIndexing(_store);
        }

        [Test]
        public void RESET_USERINDEXLUCENEANALYZER_AND_WAIT_FOR_NON_STALE()
        {
            _store.DatabaseCommands.ResetIndex("UserIndexLuceneAnalyzer");
            WaitForIndexing(_store);
        }

        public static void WaitForIndexing(IDocumentStore store, string database = null, TimeSpan? timeout = null)
        {
            var databaseCommands = store.DatabaseCommands;
            if (database != null)
            {
                databaseCommands = databaseCommands.ForDatabase(database);
            }

            timeout = timeout ?? (Debugger.IsAttached
                          ? TimeSpan.FromMinutes(5)
                          : TimeSpan.FromSeconds(20));

            if (databaseCommands.GetStatistics().Indexes.Length == 0)
                throw new Exception("Looks like you WaitForIndexing on database without indexes!");

            var spinUntil = SpinWait.SpinUntil(() =>
                    databaseCommands.GetStatistics().CountOfStaleIndexesExcludingDisabledAndAbandoned == 0,
                timeout.Value);
            if (spinUntil)
            {
                return;
            }

            var statistics = databaseCommands.GetStatistics();
            var stats = RavenJObject.FromObject(statistics).ToString(Formatting.Indented);
            var file = Path.GetTempFileName() + ".json";
            File.WriteAllText(file, stats);
            var errorMessage = string.Format("The indexes stayed stale for more than {0},{1}Details at: {2}",
                timeout.Value,
                Environment.NewLine,
                file);
            throw new TimeoutException(errorMessage);
        }
    }
}
