using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.UniqueConstraints;

namespace CCTV.RavenDB
{
    public static class DocumentStoreService
    {
        private const string ConnectionString = "http://rdb-dev.umusic.com";

        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateStore()
        {
            var store = new DocumentStore()
            {
                Url = ConnectionString,
                DefaultDatabase = "UMGDemo"
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
            store.Initialize();

            return store;
        }
    }
}
