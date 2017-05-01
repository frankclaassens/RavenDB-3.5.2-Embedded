using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CCTV.Entities;
using CCTV.Entities.Users;

using NUnit.Framework;
using CCTV.RavenDB;
using CCTV.RavenDB.Indexes;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class UserIndexTests
    {
        private readonly IDocumentStore _store = DocumentStoreService.Store;

        [Test]
        public void Can_Connect_To_Raven_Server__Through_Client_Connection()
        {
            using (var session = _store.OpenSession())
            {
                var result = session.Query<InternalUser>().ProjectFromIndexFieldsInto<InternalUser>().Take(10).ToList();
                Assert.AreEqual(10, result.Count);
            }
        }

        [Test]
        public void Confirm_All_Internal_And_External_Users_Have_Been_Indexed()
        {
            ForceCreateUserIndex();
            WaitForIndexing(_store);

            using (var session = _store.OpenSession())
            {
                RavenQueryStatistics stats;
                var users = session.Query<UserIndex.Definition, UserIndex>()
                    .Statistics(out stats)
                    .ToList();

                Assert.AreEqual(1815, stats.TotalResults);
                Assert.IsFalse(stats.IsStale);
            }
        }

        [TestCase("Michael", 18)] // sum (internal + external)
        [TestCase("Michael S", 2)] // x1 external, x1 internal
        [TestCase("Michael C", 3)] // x2 Internal, x1 External
        [TestCase("Michael M", 18)] // Raven Studio returns 18 users here! THIS IS WRONG Query: (Michael* AND M* )
        [TestCase("Michael Mc", 3)] // x2 External, x1 Internal
        [TestCase("Michael McIlwaine", 1)] //x1 Internal
        public void Query_User_Index_For_Internal_Users(string queryString, int expectedUsers)
        {
            //NOTE:
            //You can toggle the below two lines on and off if you dont want to re-create the indexes for each test case
            //This allows you to then re-run the test cases and you will see that they randomly fail!

            ForceCreateUserIndex();
            WaitForIndexing(_store);

            var actualUsers = LoadInternalUserByIndex(queryString);
            var sb = new StringBuilder();

            sb.AppendLine("ACTUAL USERS FOUND");
            sb.AppendLine("==================");
            actualUsers.ForEach(x => sb.AppendLine(x));

            System.Console.WriteLine(sb.ToString());
            Assert.AreEqual(expectedUsers, actualUsers.Count());
        }

        private IList<string> LoadInternalUserByIndex(string queryString)
        {
            var allUsers = new List<string>();

            using (var session = _store.OpenSession())
            {
                var internalUsers = session.QueryUserIndex()
                    .QueryMultipleWords(x => x.Query, queryString)
                    .Where(x => x.UserType == UserTypeOption.Internal)
                    .OrderBy(x => x.DisplayName)
                    .As<InternalUser>()
                    .ToArray();

                var externalUsers =
                    session.QueryUserIndex()
                        .QueryMultipleWords(x => x.Query, queryString)
                        .Where(x => x.UserType == UserTypeOption.External)
                        .OrderBy(x => x.DisplayName)
                        .As<ExternalUser>()
                        .ToArray();

                allUsers.AddRange(internalUsers.Select(x => x.DisplayName).ToList());
                allUsers.AddRange(externalUsers.Select(x => x.DisplayName).ToList());

                return allUsers;
            }
        }

        private void ForceCreateUserIndex()
        {
            // First delete UserIndex, then create it again.
            try
            {
                _store.DatabaseCommands.DeleteIndex("UserIndex");
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot delete UserIndex", ex);
            }

            var userIndex = new UserIndex();
            userIndex.Execute(_store);
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
