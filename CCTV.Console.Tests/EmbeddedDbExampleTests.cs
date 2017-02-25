using CCTV.Console.Tests.Common;

namespace CCTV.Console.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Bogus;
    using Domain.Entities;
    using NUnit.Framework;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.UniqueConstraints;

    [TestFixture]
    public class EmbeddedDbExampleTests : EmbeddedRavenTestBase
    {   
        [SetUp]
        public void SetupTest()
        {

        }

        [TearDown]
        public void TearDownTest()
        {
            //var emp = new Faker<Employee>()
            //    .RuleFor(u => u.FirstName, f => "Frank")
            //    .RuleFor(u => u.LastName, f => "Claassens")
            //    .RuleFor(u => u.Email, (f, u) => "Frank.Claassens3@gmail.com")
            //    .Generate();

            //using (var session = Store.OpenSession())
            //{
            //    session.Store(emp);
            //    session.SaveChanges();
            //}

            //emp = new Faker<Employee>()
            //    .RuleFor(u => u.FirstName, f => "Frank")
            //    .RuleFor(u => u.LastName, f => "Claassens")
            //    .RuleFor(u => u.Email, (f, u) => "Frank.Claassens1@gmail.com")
            //    .Generate();

            //using (var session = Store.OpenSession())
            //{
            //    session.Store(emp);
            //    session.SaveChanges();
            //}

            //Task.Run(() => BulkInsert(Store));
        }

        [Test]
        public void TestMethod1()
        {
            //_seedTask.Wait();

            //Thread.Sleep(1000);

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

            //WaitForUserToContinueTheTest(debug: true, url: "http://localhost:8080/");
            Assert.AreEqual(emp.FirstName, "Ancel");
        }
    }
}
