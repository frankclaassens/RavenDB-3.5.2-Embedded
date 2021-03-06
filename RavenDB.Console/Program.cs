﻿using CCTV.Entities;
using Raven.Abstractions.Extensions;

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
        private static IDocumentStore _store;

        private static void Main(string[] args)
        {
            _store = DocumentStoreService.Store; // Connectionstring: "http://localhost:8079"

            Console.WriteLine("LOADING ALL EMPLOYEES");
            var employees = LoadAllEmployees();
            employees.ForEach(x => Console.WriteLine(x.FirstName + " " + x.LastName));

            //InsertEmployee();

            // BulkInsert(store);

            LoadUniqueConstraint();
            //QueryIndexByName();
            Console.ReadLine();
        }

        private static IEnumerable<Employee> LoadAllEmployees()
        {
            using (var session = _store.OpenSession())
            {
                var result = session.Query<Employee>().ProjectFromIndexFieldsInto<Employee>().Take(10).ToList();
                return result;
            }
        }

        private static void LoadUniqueConstraint()
        {
            Console.WriteLine("LOADING UNIQUE CONSTRAINT");
            using (var session = _store.OpenSession())
            {
                var existingUser = session.LoadByUniqueConstraint<Employee>(x => x.FirstName, "Name 0");
                Console.WriteLine("CONSTRAINT LOADED: " + existingUser.LastName);
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
                //.RuleFor(u => u.HomePhone, (f, u) => f.Phone.PhoneNumber())
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