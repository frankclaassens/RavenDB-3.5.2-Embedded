using System;
using System.Linq;
using CCTV.Console.Tests.Common;
using CCTV.Entities;
using CCTV.RavenDB;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.UniqueConstraints;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class EmbeddedDbExampleTests : EmbeddedRavenTestBase
    {
        private bool _openRavenGuiOnTearDown = false;
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
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void Given_Seeding_Success_When_Requesting_Employee_Should_Return_Employee()
        {
            //NOTE: EmbeddedRavenTestBase already seeded 10 employees into the database.
            using (var session = Store.OpenSession())
            {
                var emp = session.Load<Employee>(1);

                Assert.NotNull(emp);
            }
        }

        [Test]
        public void Given_New_Employee_When_Saved_Then_Fetch_By_Unique_Constraint()
        {
            var newEmployee = new Employee()
            {
                FirstName = "Frank",
                LastName = "Ancel",
                Email = "frank@gmail.com"
            };

            using (var session = Store.OpenSession())
            {
                session.Store(newEmployee);
                session.SaveChanges();
            }

            Employee empByUniqueConstraint;
            using (var session = Store.OpenSession())
            {
                empByUniqueConstraint = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Frank");
            }

            Assert.AreEqual(empByUniqueConstraint.FirstName, "Frank");
        }

        [Test] public void Given_Multiple_Employees_When_Fetching_By_Constraints_Should_Return_Collection()
        {
            //NOTE: EmbeddedRavenTestBase already seeded 10 employees into the database.
            Employee[] empList;
            using (var session = Store.OpenSession())
            {
                empList = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, new string[] { "Name 0", "Name 1", "Name 2" });
            }

            Assert.AreEqual(3, empList.Length);
            Assert.AreEqual(1, empList.Count(x => x.FirstName == "Name 0"));
        }

        [Test]
        public void Can_Connect_To_Embedded_Raven_From_Client_Connection()
        {
            var store = DocumentStoreService.Store;

            using (var session = store.OpenSession())
            {
                var result = session.Query<Employee>().ProjectFromIndexFieldsInto<Employee>().Take(10).ToList();
                Assert.AreEqual(10, result.Count);
            }

            using (var session = store.OpenSession())
            {
                var unqueConstraintEmployee = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Name 0");
                Assert.NotNull(unqueConstraintEmployee);
            }           
        }
    }
}
