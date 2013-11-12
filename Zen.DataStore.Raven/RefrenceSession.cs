using System;
using Raven.Client;

namespace Zen.DataStore
{
    public class RefrenceSession<TRefObject>:IDisposable
    {
        private readonly IDocumentSession _session;
        private readonly IRepository<TRefObject> _repository;

        public RefrenceSession(IRepository<TRefObject> repository, IDocumentSession session)
        {
            _repository = repository;
            _session = session;
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
        }
    }
}