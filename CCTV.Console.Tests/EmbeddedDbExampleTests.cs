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
    public class EmbeddedDbExampleTests : CustomEmbeddedRavenTestBase
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
            BulkInsert(Store);

            using (var session = Store.OpenSession())
            {
                var results = session.Load<Employee>(1);               
            }

            using (var session = Store.OpenSession())
            {
                var newEmployee = new Employee()
                {
                    FirstName = "Frank",
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

            WaitForUserToContinueTheTest(debug: true, url: "http://localhost:8080/");
            Assert.AreEqual(emp.FirstName, "Ancel");
        }

        private static List<Employee> GenerateUsers()
        {
            var list = new Faker<Employee>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.HomePhone, (f, u) => f.Phone.PhoneNumber())
                .Generate(10).ToList();


            list.First().FirstName = "Ancel";
            return list;
        }

        private static void BulkInsert(IDocumentStore store)
        {
            var list = GenerateUsers();
            //var watch = Stopwatch.StartNew();
            //watch.Start();
            //Debug.Flush();
            //Debug.WriteLine("INSERT START 100,000 users");

            using (var session = store.OpenSession())
            {
                list.ForEach(x => session.Store(x));  
                session.SaveChanges();              
            }

            //using (BulkInsertOperation bulkInsert = store.BulkInsert())
            //{
            //    list.ForEach(x => bulkInsert.Store(x));
            //}
            //watch.Stop();
            //Debug.WriteLine("INSERT END \n TIME ELAPSED {0}", watch.Elapsed);
        }
    }
}
