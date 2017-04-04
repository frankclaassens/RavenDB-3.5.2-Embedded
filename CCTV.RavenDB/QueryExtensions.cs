namespace GRM.Works.RavenDb
{
    using Raven.Client;
    using Raven.Client.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static class QueryExtensions
    {
        public static IRavenQueryable<T> QueryMultipleWords<T>(this IRavenQueryable<T> query, Expression<Func<T, object>> fieldSelector, string queryString, 
            SearchOptions options = SearchOptions.And)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return query;
            }

            queryString = queryString.Trim().Replace(" ", "* AND ").Replace(",", "* AND ");
            if (!queryString.EndsWith("*"))
                queryString = queryString + "*";

            var result = query.Search(fieldSelector, queryString, options: options, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards);
            return result;
        }
        
        //public static IDocumentQuery<T> QueryMultipleWords<T>(this IDocumentQuery<T> query, string fieldName, string queryString)
        //{
        //    if (string.IsNullOrWhiteSpace(queryString))
        //    {
        //        return query;
        //    }

        //    queryString = queryString.Trim().Replace(" ", "* AND ").Replace(",", "* AND ");
        //    if (!queryString.EndsWith("*")) queryString = queryString + "*";

        //    return query.Search(fieldName, queryString, escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
        //}        
    }
}
