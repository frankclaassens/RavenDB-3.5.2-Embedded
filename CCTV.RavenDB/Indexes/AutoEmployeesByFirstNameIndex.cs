using Raven.Abstractions.Indexing;

namespace CCTV.RavenDB.Indexes
{
    using CCTV.Entities;
    using Raven.Client.Indexes;
    using System.Linq;

    public class EmployeesByFirstNameIndex : AbstractIndexCreationTask<Employee, EmployeesByFirstNameIndex.Definition>
    {
        public EmployeesByFirstNameIndex()
        {
            Map = docs => docs.Select(x => new
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                EmployeeGroup = x.EmployeeGroup,
                Title = x.Title,
                Query = new object[] { x.Title.Replace("-", " ").Replace("(", " ").Replace(")", " "), x.Title, x.FirstName, x.LastName }
            });

            Index(x => x.Query, FieldIndexing.Analyzed);
            Analyzers.Add(x => x.Query, "Raven.Database.Indexing.LowerCaseWhitespaceAnalyzer");
        }

        public class Definition
        {
            public string Query { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int EmployeeGroup { get; set; }

            public string Title { get; set; }
        }
    }

    //public class EmployeesByFirstNameIndex : AbstractIndexCreationTask
    //{
    //    public override string IndexName
    //    {
    //        get
    //        {
    //            return "Employees/ByFirstName";
    //        }
    //    }
    //    public override IndexDefinition CreateIndexDefinition()
    //    {
    //        return new IndexDefinition
    //        {
    //            Map = @"from doc in docs.Employees
    //                    select new {
    //                     FirstName = doc.FirstName,
    //                        LastName = doc.LastName,
    //                        HomePhone = doc.HomePhone,
    //                        Birthday = doc.Birthday,
    //                        Notes = doc.Notes,
    //                        Title = doc.Title
    //                    }"
    //        };

    //        Index(x => x.Query, FieldIndexing.Analyzed);
    //    }
    //}
}
