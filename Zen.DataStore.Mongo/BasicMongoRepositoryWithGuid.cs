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

        /// <summary>
        ///     Найти объект по GUID
        /// </summary>
        /// <param name="guid">Уникальный ИД объекта</param>
        /// <returns></returns>
        public T Find(Guid guid)
        {
            return Query.FirstOrDefault(e => e.Guid == guid);
        }

        public IQueryable<T> Find(IEnumerable<Guid> guids)
        {
            return Query.Where(e => guids.Contains(e.Guid));
        }

        /// <summary>
        ///     Клонировать документ
        /// </summary>
        /// <param name="entity">Документ, который нужно клонировать</param>
        public void Clone(T entity)
        {
            throw new NotImplementedException();
        }
    }
}