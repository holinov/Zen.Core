using System;
using System.ComponentModel;

namespace Zen.DataStore
{
    [Serializable]
    public abstract class HasGuidId : HasStringId, IHasGuidId
    {
        private Guid _guid;

        protected HasGuidId()
        {
            _guid = Guid.NewGuid();
            Id = GetType().Name + "s/" + _guid;
        }

        /// <summary>
        ///     Гуид записи
        /// </summary>
        [DisplayName("ГУИД")]
        public virtual Guid Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                Id = GetType().Name + "s/" + _guid;
            }
        }
    }
}