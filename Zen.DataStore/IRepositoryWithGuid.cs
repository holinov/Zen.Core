using System;
using System.Collections.Generic;
using System.Linq;

namespace Zen.DataStore
{
    /// <summary>
    ///     Репозитарий объектов
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepositoryWithGuid<TEntity> : IRepository<TEntity>
    {
        /// <summary>
        ///     Найти объект по GUID
        /// </summary>
        /// <param name="guid">Уникальный ИД объекта</param>
        /// <returns></returns>
        TEntity Find(Guid guid);

        IQueryable<TEntity> Find(IEnumerable<Guid> guids);

        /// <summary>
        ///     Клонировать документ
        /// </summary>
        /// <param name="entity">Документ, который нужно клонировать</param>
        void Clone(TEntity entity);

        /// <summary>
        ///     Открепить
        /// </summary>
        /// <param name="entity">Документ</param>
        void Detach(TEntity entity);

        void DeleteById(string id);

        IEnumerable<TEntity> GetAll();
    }
}