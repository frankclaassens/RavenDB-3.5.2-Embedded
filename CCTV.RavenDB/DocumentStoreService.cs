using System;
using Raven.Client;
using Raven.Client.Document;

namespace CCTV.RavenDB
{
    using Raven.Client.UniqueConstraints;

    public static class DocumentStoreService
    {
        private const string ConnectionString = "http://localhost:8079";

        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateStore()
        {
            var store = new DocumentStore()
            {
                Url = ConnectionString,
                DefaultDatabase = "StudioHub"
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
            store.Initialize();

            return store;
        }
    }
}
