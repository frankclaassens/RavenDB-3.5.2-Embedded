namespace CCTV.RavenDB.Indexes
{
    using Raven.Abstractions.Indexing;
    using Raven.Client.Indexes;

    public class AutoEmployeesByFirstNameIndex : AbstractIndexCreationTask
    {
        public override string IndexName
        {
            get
            {
                return "Auto/Employees/ByFirstName";
            }
        }
        public override IndexDefinition CreateIndexDefinition()
        {
            return new IndexDefinition
            {
                Map = @"from doc in docs.Employees
select new {
	FirstName = doc.FirstName,
    LastName = doc.LastName,
    HomePhone = doc.HomePhone,
    Birthday = doc.Birthday,
    Notes = doc.Notes,
}"
            };
        }
    }
}
