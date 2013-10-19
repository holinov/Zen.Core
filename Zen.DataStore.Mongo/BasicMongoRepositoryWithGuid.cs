using System;
using System.Collections.Generic;
using System.Linq;

namespace Zen.DataStore.Mongo
{
    public class BasicMongoRepositoryWithGuid<T> : BasicMongoRepository<T>, IRepositoryWithGuid<T> where T : IHasGuidId
    {
        public BasicMongoRepositoryWithGuid(MongoDbContext dbContext) : base(dbContext)
        {
        }

        public T Find(Guid guid)
        {
            return Query.FirstOrDefault(e => e.Guid == guid);
        }

        public IQueryable<T> Find(IEnumerable<Guid> guids)
        {
            return Query.Where(e => guids.Contains(e.Guid));
        }

        public void Clone(T entity)
        {
            throw new NotImplementedException();
        }
    }
}