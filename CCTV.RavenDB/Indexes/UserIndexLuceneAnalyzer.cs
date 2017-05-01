using System.Linq;
using CCTV.Entities.Users;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace CCTV.RavenDB.Indexes
{
    public class UserIndexLuceneAnalyzer : AbstractMultiMapIndexCreationTask<UserIndex.Definition>
    {
        public UserIndexLuceneAnalyzer()
        {
            AddMap<InternalUser>(
                docs =>
                    docs.Select(
                        x =>
                            new Definition
                            {
                                DisplayName = x.DisplayName,
                                Query = x.DisplayName.ToLowerInvariant(),
                                UserType = x.UserType,
                                ExternalUserType = ExternalUserType.Undefined
                            }));

            AddMap<ExternalUser>(
                docs =>
                    docs.Select(
                        x =>
                            new Definition
                            {
                                DisplayName = x.DisplayName,
                                Query = x.DisplayName.ToLowerInvariant(),
                                UserType = x.UserType,
                                ExternalUserType = x.ExternalUserType
                            }));

            Index(x => x.Query, FieldIndexing.Analyzed);
            Analyzers.Add(x => x.Query, "Lucene.Net.Analysis.WhitespaceAnalyzer");
        }

        public class Definition
        {
            public string DisplayName { get; set; }

            public string Query { get; set; }

            public UserTypeOption UserType { get; set; }

            public ExternalUserType ExternalUserType { get; set; }
        }
    }
}
