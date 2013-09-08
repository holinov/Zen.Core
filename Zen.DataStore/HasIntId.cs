using System;

namespace Zen.DataStore
{
    [Serializable]
    public abstract class HasIntId : IHasIntId
    {
        /// <summary>
        ///     Ид записи
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        ///     ИД сегмента
        /// </summary>
        public virtual string SegmentId { get; set; }
    }
}