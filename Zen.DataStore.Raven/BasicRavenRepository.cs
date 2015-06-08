using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace Zen.DataStore.Raven
{
    public class BasicRavenRepository<TEntity> : IRepository<TEntity>
        where TEntity : IHasStringId
    {
        private const int MaxChunkSize = 1024;

        private static readonly List<Expression<Func<TEntity, object>>> Includes =
            new List<Expression<Func<TEntity, object>>>();

        private readonly IDocumentSession _session;
        private readonly IBasicRavenRepositoryConfiguration _configuration;

        static BasicRavenRepository()
        {
            BuildIncludes();
        }

        public BasicRavenRepository(IDocumentSession session, IBasicRavenRepositoryConfiguration configuration)
        {
            _session = session;
            _configuration = configuration;
        }

        protected IDocumentSession Session
        {
            get { return _session; }
        }

        public IQueryable<TEntity> Find(IEnumerable<string> ids)
        {
            return Session.Load<TEntity>(ids.ToArray()).AsQueryable();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            //if (_session != null) _session.Dispose();
        }

        /// <summary>
        ///     Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        public TEntity Find(string id)
        {
            dynamic ldr = MakeIncludes();
            if (ldr != null && id != null) return ldr.Load<TEntity>(id);
            return default(TEntity);
        }

        /// <summary>
        ///     Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Store(TEntity entity)
        {
            Session.Store(entity);
        }

        /// <summary>
        ///     Удалить объект из БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Delete(TEntity entity)
        {
            Session.Delete(entity);
        }

        /// <summary>
        ///     Сохранить изменения сессии
        /// </summary>
        public void SaveChanges()
        {
            Session.SaveChanges();
        }

        /// <summary>
        ///     Постоить запрос
        /// </summary>
        public IQueryable<TEntity> Query
        {
            get
            {
                if (Session != null)
                {
                    IRavenQueryable<TEntity> ss = Session.Query<TEntity>();
                    if (_configuration.WaitForStaleIndexes)
                        ss = ss.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite());

                    foreach (var include in Includes)
                    {
                        Expression<Func<TEntity, object>> include1 = include;
                        ss.Customize(x => x.Include(include1));
                    }
                    
                    return ss;
                }
                return null;
            }
        }

        public IEnumerable<TEntity> Enumerate(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter=null, int pageSize = 120)
        {
            RavenQueryStatistics stats;
            var session = OpenClonedSession(Session);
            var sessQueryCount = 1;
            
            IRavenQueryable<TEntity> ravenQuery = Session.Query<TEntity>().Statistics(out stats);

            IQueryable<TEntity> results = !_configuration.WaitForStaleIndexes
                ? ravenQuery
                : ravenQuery.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite()); //ДЛЯ ТЕСТОВ                                    
                                                 
            if (filter != null)
                results = filter(results);

            results = results
                .Skip(0 * pageSize) // retrieve results for the first page
                .Take(pageSize); // page size is 10;

            var pages = (stats.TotalResults - stats.SkippedResults) / pageSize;

            foreach (var entity in results)
                yield return entity;
            
            try
            {
                for (int i = 1; i <= pages; i++)
                {
                    if (sessQueryCount++ >= session.Advanced.MaxNumberOfRequestsPerSession)
                    {
                        session.Dispose();
                        session = OpenClonedSession(Session);
                    }

                    ravenQuery = session.Query<TEntity>()
                        .Statistics(out stats);

                    results = !_configuration.WaitForStaleIndexes
                        ? ravenQuery
                        : ravenQuery.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite()); //ДЛЯ ТЕСТОВ
                    
                    if (filter != null)
                        results = filter(results);

                    results = results
                        .Skip(i * pageSize + stats.SkippedResults)
                        .Take(pageSize);

                    foreach (var entity in results)
                        yield return entity;
                }
            }
            finally
            {
                session.Dispose();
            }
            
        }

        private IDocumentSession OpenClonedSession(IDocumentSession session)
        {
            return session.Advanced.DocumentStore.OpenSession();
        }

        private static IQueryable<TEntity> FilterOut(IQueryable<TEntity> results)
        {
            return results.Where(x => x.Id != null);
        }

        public void DeleteAttach(string key)
        {
            Session.Advanced.DocumentStore.DatabaseCommands.DeleteAttachment(key, null);
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            int numberOfObjectsThatWarrantChunking = 2000;

            if (entities.Count() < numberOfObjectsThatWarrantChunking)
            {
                foreach (var entity in entities)
                    Session.Store(entity);
                Session.SaveChanges();
                return;
            }

            int numberOfDocumentsPerSession = MaxChunkSize;

            var objectListInChunks = new List<List<TEntity>>();

            for (int i = 0; i < entities.Count(); i += numberOfDocumentsPerSession)
            {
                objectListInChunks.Add(entities.Skip(i).Take(numberOfDocumentsPerSession).ToList());
            }

            Parallel.ForEach(objectListInChunks, listOfObjects =>
                {
                    using (IDocumentSession ravenSession = Session.Advanced.DocumentStore.OpenSession())
                    {
                        listOfObjects.ForEach(x => ravenSession.Store(x));
                        ravenSession.SaveChanges();
                    }
                });
        }

        /// <summary>
        ///     Постоение списка включений в запрос
        /// </summary>
        private static void BuildIncludes()
        {
            PropertyInfo[] refs = typeof (TEntity).GetProperties()
                                                  .Where(p => p.PropertyType.GetInterface("IRefrence", true) != null)
                                                  .ToArray();


            foreach (var refObject in refs)
            {
                PropertyInfo refObjectIdMemberInfo = refObject.PropertyType.GetProperty("Id");
                ParameterExpression inp = Expression.Parameter(typeof (TEntity));
                MemberExpression accessObj = Expression.MakeMemberAccess(inp, refObject);
                MemberExpression accessObjId = Expression.MakeMemberAccess(accessObj, refObjectIdMemberInfo);
                Expression<Func<TEntity, object>> func = Expression.Lambda<Func<TEntity, object>>(accessObjId, inp);
                Includes.Add(func);
            }
        }

        /// <summary>
        ///     Построить загрузчик с включениями
        /// </summary>
        /// <returns>Загрузчик</returns>
        protected virtual dynamic MakeIncludes()
        {
            if (Session == null)
                return null;

            dynamic loader = Session;
            foreach (var expression in Includes)
            {
                if (loader is IDocumentSession)
                {
                    loader = ((IDocumentSession) loader).Include(expression);
                }
                else
                {
                    loader = ((ILoaderWithInclude<TEntity>) loader).Include(expression);
                }
            }
            return loader;
        }

        public void Detach(TEntity entity)
        {
            Session.Advanced.Evict(entity);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return QueryAll<TEntity>(x => x, null);
        }

        public IQueryable<TEntity> QueryAll(Expression<Func<TEntity, bool>> queryExpr = null)
        {
            return QueryAll<TEntity>(x => x, queryExpr);
        }

        public IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selectExpr, Expression<Func<TEntity, bool>> queryExpr = null)
        {
            List<TResult> allEntities = new List<TResult>();
            
            var session = OpenClonedSession(Session);
            try
            {
                int queryCount = 0;
                int lastCount = 0;
                while (true)
                {
                    if (++queryCount >= session.Advanced.MaxNumberOfRequestsPerSession)
                    {
                        session.Dispose();
                        session = null;

                        session = OpenClonedSession(Session);
                    }

                    RavenQueryStatistics statistics;
                    var query = session.Query<TEntity>().Statistics(out statistics);
                    if (_configuration.WaitForStaleIndexes)
                        query = query.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite());

                    if (queryExpr != null)
                        query = query.Where(queryExpr);

                    allEntities.AddRange(query.Select(selectExpr).Skip(allEntities.Count).Take(MaxChunkSize));

                    if (allEntities.Count - lastCount < MaxChunkSize)
                        break;

                    if (lastCount == 0 && statistics.TotalResults > allEntities.Count)
                    {
                        var newList = new List<TResult>(statistics.TotalResults);
                        newList.AddRange(allEntities);

                        allEntities = newList;
                    }

                    lastCount = allEntities.Count;
                }
            }
            finally
            {
                if (session != null)
                    session.Dispose();
            }

            return allEntities.AsQueryable();
        }

        public void DeleteById(string id)
        {
            Session.Advanced.Defer(new DeleteCommandData { Key = id });
        }
    }
}