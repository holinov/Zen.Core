using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zen.DataStore.Raven
{
    public static class RavenQueryableExtensions
    {
        public static IEnumerable<T> FetchAll<T>(this IQueryable<T> queryable)
        {
            var ravenQueryable = queryable as IRavenQueryable<T>;
            if (ravenQueryable == null)
                throw new InvalidOperationException("Cannot handle anything other than an IRavenQueryable<T>");

            const int numberToTake = 1024;
            var result = new List<T>();
            int numberToSkip = 0;

            RavenQueryStatistics stat;
            result.AddRange(ravenQueryable.Statistics(out stat).Skip(numberToSkip).Take(numberToTake));
            numberToSkip += 1024;

            while (numberToSkip < stat.TotalResults)
            {
                result.AddRange(ravenQueryable.Skip(numberToSkip).Take(numberToTake));
                numberToSkip += 1024;
            }

            return result;
        }
    }
}
