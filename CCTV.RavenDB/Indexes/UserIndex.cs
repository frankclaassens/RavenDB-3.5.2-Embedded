using System.Linq;
using CCTV.Entities.Users;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace CCTV.RavenDB.Indexes
{
    public class UserIndex : AbstractMultiMapIndexCreationTask<UserIndex.Definition>
    {
        public UserIndex()
        {
            AddMap<InternalUser>(
                docs =>
                    docs.Select(
                        x =>
                            new Definition
                            {
                                DisplayName = x.DisplayName,
                                Query = x.DisplayName,
                                UserType = x.UserType,
                                ExternalUserType = ExternalUserType.Undefined,
                            }));

            AddMap<ExternalUser>(
                docs =>
                    docs.Select(
                        x =>
                            new Definition
                            {
                                DisplayName = x.DisplayName,
                                Query = x.DisplayName,
                                UserType = x.UserType,
                                ExternalUserType = x.ExternalUserType
                            }));

            Index(x => x.Query, FieldIndexing.Analyzed);
            Analyzers.Add(x => x.Query, "Raven.Database.Indexing.LowerCaseWhitespaceAnalyzer");
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
