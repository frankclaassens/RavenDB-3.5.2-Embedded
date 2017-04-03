using System;
using System.Collections;
using System.Collections.Generic;
using CCTV.Console.Tests.Common;
using CCTV.Entities;
using GRM.Works.RavenDb;
using NUnit.Framework;
using Raven.Client.Linq;
using Raven.Client;
using System.Linq;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class AdhocTests : EmbeddedRavenTestBase
    {
        [OneTimeSetUp]
        public void TestSetup()
        {
            var employees = new List<Employee>();
            employees.Add(CreateEmployee("Name 1", "Deal With Option", 1));
            employees.Add(CreateEmployee("Name 3", "Deal With Option", 1));
            employees.Add(CreateEmployee("Name 5", "Deals", 1));
            employees.Add(CreateEmployee("Name 6", "Deal But No Deal", 1));
            employees.Add(CreateEmployee("Name 7", "Deal With", 1));
            employees.Add(CreateEmployee("Name 8", "Deal", 1));
            employees.Add(CreateEmployee("Name 9", "No Deals", 1));

            employees.Add(CreateEmployee("Name 10", "Something Cool", 2));
            employees.Add(CreateEmployee("Name 2", "Deal With Option", 2));
            employees.Add(CreateEmployee("Name 4", "Deals", 2));

            using (var session = Store.OpenSession())
            {
                employees.ForEach(x => session.Store(x));
                session.SaveChanges();
            }
        }

        [TestCase("Deal", 6)]
        [TestCase("Deal With Option", 2)]
        [TestCase("Deal With", 3)]
        public void QueryMultipleWordsTest(string queryString, int count)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<Employee>().Where(x => x.EmployeeGroup == 1);

                var results = query.QueryMultipleWords(employee => employee.Title, queryString).OrderBy(x => x.FirstName).ToArray();

                Assert.AreEqual(count, results.Count());
            }
        }

        private Employee CreateEmployee(string firstname, string title, int employeeGroup)
        {
            return new Employee()
            {
                FirstName = firstname,
                Title = title,
                EmployeeGroup = employeeGroup
            };
        }
    }
}
