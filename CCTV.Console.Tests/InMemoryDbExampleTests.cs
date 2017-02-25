using CCTV.Console.Tests.Common;

namespace CCTV.Console.Tests
{
    using System;
    using CCTV.Domain.Entities;
    using NUnit.Framework;
    using Raven.Client.UniqueConstraints;

    [TestFixture]
    public class InMemoryDbExampleTests : InMemoryRavenTestBase
    {   
        [OneTimeSetUp]
        public void SetUp()
        {
            StartRavenStudio = false;
            _seedTask.Wait();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (StartRavenStudio)
                WaitForUserToContinueTheTest(debug: false, url: "http://localhost:8080/");
        }

        [Test]
        public void TestMethod1()
        {
            TestContext.WriteLine("Running Test");

            using (var session = Store.OpenSession())
            {
                var results = session.Load<Employee>(1);               
            }

            using (var session = Store.OpenSession())
            {
                var newEmployee = new Employee()
                {
                    FirstName = "Ancel",
                    LastName = "Claassens",
                    Email = "frank@gmail.com"
                };
                session.Store(newEmployee);
                session.SaveChanges();
            }

            Employee emp;
            using (var session = Store.OpenSession())
            {
                emp = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Ancel");
            }
            
            Assert.AreEqual(emp.FirstName, "Ancel");
        }
    }
}
