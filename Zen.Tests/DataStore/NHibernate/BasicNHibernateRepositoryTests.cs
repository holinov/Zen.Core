/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Zen.DataStore;
using Zen.DataStore.NHibernate;
using Zen.DataStore.NHibernate.SQLLite;
using Zen.DataStore.Raven;
using Zen.DataStore.Raven.Embeeded;
using Zen.Tests.DataStore.DataModel;
*/

namespace Zen.Tests.DataStore.NHibernate
{
    /*[TestFixture]
    public class BasicNHibernateRepositoryTests
    {

        private AppCore _core;

        [TestFixtureSetUp]
        public void Start()
        {
            if (_core == null)
            {
                _core = AppCoreBuilder
                    .Create()
                    .AddModule(new NHibernateSQLiteDatastoreModule(this.GetType().Assembly))
                    .AddModule<NHibernateRepositoriesModule>()
                    .Configure(b =>
                    {
                        // Необходимо для работы AutofacCreationConverter
                        b.RegisterType<TestObject>();
                        b.RegisterType<TestObject1>();
                    })
                    .Build();
            }
        }

        [TestFixtureTearDown]
        public void Stop()
        {
            _core.Dispose();
        }

        [Test]
        public void BasicNHibernateRepositoryTest()
        {
            string realId;
            using (var scope = _core.BeginScope())
            {
                using (var repos = scope.Resolve<IRepositoryWithGuid<TestObject>>())
                using (var repos1 = scope.Resolve<IRepositoryWithGuid<TestObject>>())
                {
                    var item = new TestObject
                        {
                            Name = "123",
                        };
                    repos.Store(item);
                    repos.SaveChanges();
                    realId = item.Id;
                    Assert.IsNotNullOrEmpty(realId);

                    var items = repos1.Query.First();
                    Assert.NotNull(items);

                }

                using (var repos = scope.Resolve<IRepositoryWithGuid<TestObject>>())
                {
                    var items = repos.Query.First();
                    Assert.NotNull(items);
                }
            }

        }

        [Test]
        public void BasicNHibernateRepositoryCreationTest()
        {
            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            {
                Assert.NotNull(repos);
            }
        }

        [Test]
        public void BasicNHibernateRepositoryBulkOperationsTest()
        {
            var items = new List<TestObject>();
            for (int i = 0; i < 3000; i++)
            {
                items.Add(new TestObject
                {
                    Name = "Tst" + i,
                    Guid = Guid.NewGuid(),
                });
            }

            using (var repo = _core.Resolve<IRepository<TestObject>>())
            {
                repo.StoreBulk(items);                
            }

            using (var repo = _core.Resolve<IRepository<TestObject>>())
            {
                Assert.AreEqual(items.Count, repo.GetAll().ToArray().Count());
            }
        }
    }*/
}
