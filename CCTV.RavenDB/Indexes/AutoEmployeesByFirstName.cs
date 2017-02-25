using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTV.RavenDB.Indexes
{
    using Raven.Abstractions.Indexing;
    using Raven.Client.Indexes;

    public class AutoEmployeesByFirstName : AbstractIndexCreationTask
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
	FirstName = doc.FirstName
}"
            };
        }
    }
}
