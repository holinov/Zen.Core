using System;
using System.ComponentModel;

namespace Zen.DataStore
{
    [Serializable]
    public abstract class HasGuidId : HasStringId, IHasGuidId
    {
        protected HasGuidId()
        {
            Guid = Guid.NewGuid();
        }
        private Guid _guid;

        /// <summary>
        /// Гуид записи
        /// </summary>
        [DisplayName("ГУИД")]
        public Guid Guid
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
