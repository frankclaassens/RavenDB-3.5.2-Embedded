using CCTV.Entities;

namespace RavenDB.Console.App
{
    using System.Diagnostics;
    using CCTV.RavenDB;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bogus;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Linq;
    using Raven.Client.UniqueConstraints;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.WindowStyle = ProcessWindowStyle.Normal;

            var store = DocumentStoreService.Store;

            //InsertEmployee();

            //BulkInsert(store);

            LoadUniqueCon();
            QueryIndexByName();

        }

        private static void LoadUniqueCon()
        {

            using (var session = DocumentStoreService.Store.OpenSession())
            {
                var existingUser = session
               .LoadByUniqueConstraint<Employee>(x => x.Email, "Frank@mail.com");
            }
               
        }

        private static void InsertEmployee()
        {
            using (var session = DocumentStoreService.Store.OpenSession())
            {
                var emp = new Faker<Employee>()
                    .RuleFor(u => u.FirstName, f => "Frank")
                    .RuleFor(u => u.LastName, f => "Claassens")
                    .RuleFor(u => u.Email, (f, u) => "Frank2@mail.com")
                    .Generate();

                session.Store(emp);
                session.SaveChanges();
            }
        }

        private static void QueryIndexByName()
        {
            using (var session = DocumentStoreService.Store.OpenSession())
            {
                RavenQueryStatistics stats;
                IList<Employee> results = session
                    .Query<Employee>("Auto/Employees/ByFirstName")
                     .Where(x => x.FirstName == "Emory")
                     .Statistics(out stats)
                     .Customize(x => x.WaitForNonStaleResults())
                     .ToList();

                Console.WriteLine("Index State State: {0}", stats.IsStale);

                Console.WriteLine(results.Count);
                Console.ReadKey();
            }
        }

        private static List<Employee> GenerateUsers()
        {
            var list = new Faker<Employee>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.HomePhone, (f, u) => f.Phone.PhoneNumber())
                .Generate(100).ToList();

            return list;
        }

        private static void BulkInsert(IDocumentStore store)
        {
            var list = GenerateUsers();
            using (BulkInsertOperation bulkInsert = store.BulkInsert())
            {
                list.ForEach(x => bulkInsert.Store(x));
            }
            Console.WriteLine("Created 10000 users");
        }
    }
}