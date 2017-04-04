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
using CCTV.RavenDB.Indexes;

namespace CCTV.Console.Tests
{
    [TestFixture]
    public class AdhocTests : EmbeddedRavenTestBase
    {
        private bool _openRavenGuiOnTearDown = true;
        private bool _pauseRavenGui = true;

        [OneTimeSetUp]
        public void TestSetup()
        {
            var employees = new List<Employee>();
            // EMPLOYEE GROUP 1
            employees.Add(CreateEmployee("Name 1", "Deal With Option", 1));
            employees.Add(CreateEmployee("Name 3", "Deal With Option", 1));
            employees.Add(CreateEmployee("Name 5", "Deals", 1));
            employees.Add(CreateEmployee("Name 6", "Deal But No Deal", 1));
            employees.Add(CreateEmployee("Name 7", "Deal With", 1));
            employees.Add(CreateEmployee("Name 8", "Deal", 1));
            employees.Add(CreateEmployee("Name 9", "No Deals", 1));
            employees.Add(CreateEmployee("Name 11", "Something Definitely Cool", 1));
            employees.Add(CreateEmployee("Name 12", "Cool and not Cool 123", 1));

            // EMPLOYEE GROUP 2
            employees.Add(CreateEmployee("Name 10", "Something Cool", 2));
            employees.Add(CreateEmployee("Name 2", "Deal With Option", 2));
            employees.Add(CreateEmployee("Name 4", "Deals", 2));

            using (var session = Store.OpenSession())
            {
                employees.ForEach(x => session.Store(x));
                session.SaveChanges();
            }
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
        }

        [TestCase("Deal", 7)]
        [TestCase("Deal With Option", 2)]
        [TestCase("Deal With", 3)]
        [TestCase("Cool", 2)]
        [TestCase("Definitely Cool", 1)]
        public void QueryMultipleWordsTestForEmployeeGroup1(string queryString, int count)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Query<EmployeesByFirstNameIndex.Definition, EmployeesByFirstNameIndex>().Where(x => x.EmployeeGroup == 1);

                var results = query.QueryMultipleWords(employee => employee.Query, queryString).ToArray();

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
