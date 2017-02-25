using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;

namespace CCTV.RavenDB
{
    using Raven.Client.UniqueConstraints;

    public static class DocumentStoreService
    {
        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateStore()
        {
            var store = new DocumentStore()
            {
                Url = "http://localhost:80",
                DefaultDatabase = "Northwind"
            };

            store.RegisterListener(new UniqueConstraintsStoreListener());
            store.Initialize();

            return store;
        }
    }
}
