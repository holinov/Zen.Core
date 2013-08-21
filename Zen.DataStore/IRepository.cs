using System;
using System.Collections.Generic;
using System.Linq;

namespace Zen.DataStore
{
    /// <summary>
    /// Репозитарий объектов
    /// </summary>
    /// <typeparam name="TEntity">Тип объекта</typeparam>
    public interface IRepository<TEntity>: IDisposable
    {
        /// <summary>
        /// Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        TEntity Find(string id);

        IQueryable<TEntity> Find(IEnumerable<string> ids);

        /// <summary>
        /// Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        void Store(TEntity entity);

        void StoreBulk(IEnumerable<TEntity> entities);
        /// <summary>
        /// Удалить объект из БД
        /// </summary>
        /// <param name="entity">Объект</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Сохранить изменения сессии
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Постоить запрос
        /// </summary>
        IQueryable<TEntity> Query { get; }

        void DeleteAttach(string key);
    }
}