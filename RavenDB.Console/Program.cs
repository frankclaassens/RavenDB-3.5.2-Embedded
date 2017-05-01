using CCTV.Entities;
using CCTV.Entities.Users;
using CCTV.RavenDB.Indexes;
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
            _store = DocumentStoreService.Store; // NOTE: Change Connectionstring property in DocumentStoreService class.

            Console.WriteLine("LOADING ALL EMPLOYEES");
            var employees = LoadAllEmployees();
            employees.ForEach(x => Console.WriteLine(x.FirstName + " " + x.LastName));



            var user = LoadInternalUser(Guid.Parse("63f31ca4-9fd3-4b08-a3ff-6ada9989b480"));
            var userByIndex = LoadInternalUserByIndex("Michael Mc");

            //InsertEmployee();

            // BulkInsert(store);

            LoadUniqueConstraint();
            //QueryIndexByName();
            Console.ReadLine();
        }

        private static InternalUser[] LoadInternalUserByIndex(string queryString)
        {
            using (var session = _store.OpenSession())
            {
                var result = session.QueryUserIndex()
                    .QueryMultipleWords(x => x.Query, queryString)
                    .Where(x => x.UserType == UserTypeOption.Internal)
                    .As<InternalUser>()
                    .ToArray();

                return result;
            }
        }

        //public ActionResult SearchAllUsers(UserSearchModel model)
        //{
        //    if (string.IsNullOrWhiteSpace(model.Name))
        //    {
        //        throw new SimpleValidationException("Search criteria is mandatory.");
        //    }

        //    var internalResultsQuery =
        //        RavenSession.QueryUserIndex()
        //            .Customize(x => x.Include<InternalUser, LabelFamily>(i => i.LabelFamilyRef.Id))
        //            .Where(x => x.UserType == UserTypeOption.Internal);

        //    //Some admins are restricted to specific label families
        //    var permittedLabelFamilyIds = GetPermittedImpersonationLabelFamilyIds();
        //    if (permittedLabelFamilyIds.Any())
        //    {
        //        internalResultsQuery = internalResultsQuery.Where(x => x.LabelFamilyId.HasValue && x.LabelFamilyId.Value.In(permittedLabelFamilyIds));
        //    }

        //    var internalResults = internalResultsQuery
        //        .QueryMultipleWords(x => x.Query, model.Name)
        //        .OrderBy(x => x.DisplayName)
        //        .As<InternalUser>()
        //        .ToArray();

        //    var externalResults =
        //        RavenSession.QueryUserIndex()
        //            .Customize(x => x.Include<ExternalUser, Contact>(i => i.ContactRef.Id))
        //            .Where(x => x.UserType == UserTypeOption.External)
        //            .QueryMultipleWords(x => x.Query, model.Name)
        //            .OrderBy(x => x.DisplayName)
        //            .As<ExternalUser>()
        //            .ToArray();

        //    return BuildAdminUserResources(internalResults, externalResults);
        //}

        private static InternalUser LoadInternalUser(Guid internalUserId)
        {
            using (var session = _store.OpenSession())
            {
                var result = session.Load<InternalUser>(internalUserId);

                return result;
            }
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