using System;
using Raven.Client;

namespace Zen.DataStore
{
    public class RefrenceSession<TRefObject>:IDisposable
    {
        private readonly IDocumentSession _session;
        private readonly IRepository<TRefObject> _repository;
        private readonly IAppScope _scope;

        public RefrenceSession(IRepository<TRefObject> repository, IDocumentSession session, IAppScope scope = null)
        {
            _repository = repository;
            _session = session;
            _scope = scope;
        }

        public IRepository<TRefObject> Repository
        {
            get { return _repository; }
        }

        public IDocumentSession Session
        {
            get { return _session; }
        }

        public void Dispose()
        {
            Repository.Dispose();
            _session.Dispose();
            if (_scope != null)
                _scope.Dispose();
        }
    }
}