using System;

namespace Zen.DataStore
{
    /// <summary>
    ///     Объект с GUID идентификатором
    /// </summary>
    public interface IHasGuidId : IHasStringId
    {
        /// <summary>
        ///     Гуид записи
        /// </summary>
        Guid Guid { get; set; }
    }
}