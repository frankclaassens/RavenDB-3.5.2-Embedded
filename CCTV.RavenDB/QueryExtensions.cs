using System;
using System.Linq.Expressions;
using CCTV.RavenDB.Indexes;
using Raven.Client;
using Raven.Client.Linq;

namespace CCTV.RavenDB
{
    public static class QueryExtensions
    {
        public static IRavenQueryable<T> QueryMultipleWords<T>(this IRavenQueryable<T> query, Expression<Func<T, object>> fieldSelector, string queryString, 
            SearchOptions options = SearchOptions.And)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return query;
            }

            queryString = queryString.Trim().Replace(" ", "* AND ");
            if (!queryString.EndsWith("*"))
            {
                queryString = queryString + "*";
            }                

            var result = query.Search(fieldSelector, queryString, options: options, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards);
            return result;
        }

        // THIS METHOD IS USED ALONGSIDE THE LUCENE.NET WhiteSpaceAnalyzer to mimic the behavior of a LowerCaseWhiteSpaceAnalizer
        public static IRavenQueryable<T> QueryMultipleWordsToLowerVarient<T>(this IRavenQueryable<T> query, Expression<Func<T, object>> fieldSelector, string queryString,
            SearchOptions options = SearchOptions.And)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return query;
            }

            queryString = queryString.Trim().ToLowerInvariant().Replace(" ", "* AND ");
            if (!queryString.EndsWith("*"))
            {
                queryString = queryString + "*";
            }                

            var result = query.Search(fieldSelector, queryString, options: options, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards);
            return result;
        }

        public static IRavenQueryable<UserIndex.Definition> QueryUserIndex(this IDocumentSession ravenSession)
        {
            return ravenSession.Query<UserIndex.Definition, UserIndex>();
        }

        public static IRavenQueryable<UserIndexLuceneAnalyzer.Definition> QueryUserIndexLuceneAnalyzer(this IDocumentSession ravenSession)
        {
            return ravenSession.Query<UserIndexLuceneAnalyzer.Definition, UserIndexLuceneAnalyzer>();
        }   
    }
}
