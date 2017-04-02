using CCTV.Console.Tests.Common;
using CCTV.Entities;
using NUnit.Framework;
using Raven.Client.UniqueConstraints;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class EmbeddedDbExampleTests : EmbeddedRavenTestBase
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            StartRavenStudio = false;
            //_seedTask.Wait();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (StartRavenStudio)
                WaitForUserToContinueTheTest(debug: true, url: "http://localhost:8080/");
        }

        [Test]
        public void TestMethod1()
        {
            using (var session = Store.OpenSession())
            {
                var results = session.Load<Employee>(1);
            }

            using (var session = Store.OpenSession())
            {
                var newEmployee = new Employee()
                {
                    FirstName = "Frank",
                    LastName = "Ancel",
                    Email = "frank@gmail.com"
                };
                session.Store(newEmployee);
                session.SaveChanges();
            }

            Employee emp;
            using (var session = Store.OpenSession())
            {
                emp = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Frank");
            }

            Employee[] empList;
            using (var session = Store.OpenSession())
            {
                empList = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, new string[] { "Frank", "Name 0", "Name 1"} );
            }

            Assert.AreEqual(emp.FirstName, "Frank");
        }
    }
}
