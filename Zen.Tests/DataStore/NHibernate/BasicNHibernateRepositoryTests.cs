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


namespace Zen.Tests.DataStore.NHibernate
{
    [TestFixture]
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
                    /*.AddModule(new RavenEmbeededDataStoreModule("Data") { RunInMemory = true })
                    .AddModule<RavenRepositoriesModule>()*/
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
            //using (var sess = _ds.OpenSession())
            using (var scope = _core.BeginScope())
            {
                using (var repos = scope.Resolve<IRepositoryWithGuid<TestObject>>())
                using (var repos1 = scope.Resolve<IRepositoryWithGuid<TestObject>>())
                    //using (var repos1 = _core.Resolve<IRepositoryWithGuid<TestObject1>>())
                {
                    var item1 = new TestObject {Name = "1233"};
                    // repos.Store(item1);
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
                    //Assert.Greater(items.Count(), 0);
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
    }
}
