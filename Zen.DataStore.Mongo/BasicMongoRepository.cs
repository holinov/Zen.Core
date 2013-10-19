using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Zen.DataStore.Mongo
{
    public class BasicMongoRepository<T> : IRepository<T> where T : IHasStringId
    {
        private readonly MongoDbContext _dbContext;
        private readonly MongoCollection<T> _collection;

        public BasicMongoRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
            _collection = _dbContext.Database.GetCollection<T>(typeof (T).Name);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Постоить запрос
        /// </summary>
        public IQueryable<T> Query
        {
            get { return _collection.AsQueryable(); }
        }

        /// <summary>
        /// Коллекция Mongo DB
        /// </summary>
        protected MongoCollection<T> Collection
        {
            get { return _collection; }
        }

        /// <summary>
        /// Контекс БД Mongo
        /// </summary>
        public MongoDbContext DbContext
        {
            get { return _dbContext; }
        }

        /// <summary>
        ///     Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        public T Find(string id)
        {
            return Query.FirstOrDefault(e => e.Id == id);
        }

        public IQueryable<T> Find(IEnumerable<string> ids)
        {
            return Query.Where(e => ids.Contains(e.Id));
        }

        /// <summary>
        ///     Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Store(T entity)
        {
            _collection.Save(entity);
        }

        public void StoreBulk(IEnumerable<T> entities)
        {
            _collection.InsertBatch(entities);
        }

        public void Delete(T entity)
        {
            DeleteById(entity.Id);
        }

        /// <summary>
        ///     Сохранить изменения сессии
        /// </summary>
        public void SaveChanges()
        {
            //TODO: Save\Rollback 
        }

        public void DeleteAttach(string key)
        {
            throw new NotImplementedException();
        }

        public void Detach(T entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(string id)
        {
            var q = MongoDB.Driver.Builders.Query<T>.EQ(e => e.Id, id);
            _collection.Remove(q);
        }

        public IEnumerable<T> GetAll()
        {
            return Query;
        }
    }
}