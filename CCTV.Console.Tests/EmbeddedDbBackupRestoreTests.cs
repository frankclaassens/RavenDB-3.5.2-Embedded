using System;
using System.IO;
using System.Linq;
using System.Threading;
using Bogus;
using CCTV.Console.Tests.Common;
using CCTV.Entities;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Client.UniqueConstraints;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class EmbeddedDbBackupRestoreTests : EmbeddedRavenTestBase
    {
        private bool _openRavenGuiOnTearDown = true;
        private bool _pauseRavenGui = true;

        [OneTimeSetUp]
        public void SetUp()
        {

        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (_openRavenGuiOnTearDown)
            {
                var studioTask = LaunchRavenStudioGui();
                if (_pauseRavenGui)
                    studioTask.Wait();
            }

            try
            {
                Store.Dispose();
                ClearDatabaseBackupFolder();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void Backup_Embedded_Test_Database()
        {
            var backupTask = BackupDatabaseAsync();

            TestContext.WriteLine(backupTask.Status);
            backupTask.Wait();
            TestContext.WriteLine(backupTask.Status);

            Assert.AreEqual("RanToCompletion", backupTask.Status.ToString());

            ClearDatabaseBackupFolder();
        }

        [Test]
        public void Restore_Embedded_Test_Database()
        {
            using (var session = Store.OpenSession())
            {
                var emp = session.Load<Employee>(1);

                Assert.AreEqual("Name 0", emp.FirstName);
            }

            InsertUniqueEmployee("Employee BEFORE Backup");

            using (var session = Store.OpenSession())
            {
                var emp = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Employee BEFORE Backup");
                Assert.AreEqual("Employee BEFORE Backup", emp.FirstName);
            }

            var backupTask = BackupDatabaseAsync();
            backupTask.Wait();

            InsertUniqueEmployee("Employee AFTER Backup");
            BulkInsert(8500);

            using (var session = Store.OpenSession())
            {
                var emp = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Employee AFTER Backup");
                Assert.AreEqual("Employee AFTER Backup", emp.FirstName);
            }

            var restoreTask = RestoreDatabaseAsync();
            restoreTask.Wait();
            Assert.AreEqual("RanToCompletion", restoreTask.Status.ToString());

            using (var session = Store.OpenSession())
            {
                var initialEmployee = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Employee BEFORE Backup");
                Assert.AreEqual("Employee BEFORE Backup", initialEmployee.FirstName);

                var lostEmployeeDueToRestore = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Employee AFTER Backup");
                Assert.IsNull(lostEmployeeDueToRestore);
            }
        }

        private void InsertUniqueEmployee(string firstName)
        {
            var employees = new Faker<Employee>()
             .RuleFor(u => u.FirstName, f => firstName)
             .RuleFor(u => u.LastName, f => f.Name.LastName())
             .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
             .RuleFor(u => u.Description, (f, u) => f.Random.AlphaNumeric(50))
             .Generate(1).ToArray();

            using (var session = Store.OpenSession())
            {
                employees.ForEach(e => session.Store(e));
                session.SaveChanges();
            }
        }

        private void BulkInsert(int recordsToInsert)
        {
            var employees = new Faker<Employee>()
             .RuleFor(u => u.FirstName, f => $"Names {f.UniqueIndex}")
             .RuleFor(u => u.LastName, f => f.Name.LastName())
             .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
             .RuleFor(u => u.Description, (f, u) => f.Random.AlphaNumeric(50))
             .Generate(recordsToInsert).ToArray();

            using (var session = Store.OpenSession())
            {
                employees.ForEach(e => session.Store(e));
                session.SaveChanges();
            }
        }

        private void ClearDatabaseBackupFolder()
        {
            Directory.GetDirectories(@"C:\EmbeddedBackup\").ForEach(x => Directory.Delete(x, true));
        }
    }
}
