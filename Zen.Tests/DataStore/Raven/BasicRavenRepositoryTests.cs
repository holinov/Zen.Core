using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Zen.DataStore;
using Zen.DataStore.Raven;
using Zen.DataStore.Raven.Embeeded;
using Zen.Tests.DataStore.DataModel;

namespace Zen.Tests.DataStore.Raven
{
    [TestFixture]
    public class BasicRavenRepositoryTests
    {
        

        
        private AppCore _core;

        [TestFixtureSetUp]
        public void Start()
        {
            if (_core == null)
            {
                _core = AppCoreBuilder
                    .Create()
                    .AddModule(new RavenEmbeededDataStoreModule("Data") {RunInMemory = true})
                    .AddModule<RavenRepositoriesModule>()
                    .Configure(b =>
                        {
                            // Необходимо для работы AutofacCreationConverter
                            b.RegisterType<TestObject>().PropertiesAutowired();
                            //b.RegisterType<TestObject1>();
                        })
                    .Build();

                AutofacCreationConverter.Container = _core.Scope;
            }
        }

        [TestFixtureTearDown]
        public void Stop()
        {
            _core.Dispose();
        }

        [Test]
        public void BasicRavenReposTest()
        {
            string realId;
            Guid realGuid;
            string refId;
            //using (var sess = _ds.OpenSession())
            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            using (var repos1 = _core.Resolve<IRepositoryWithGuid<TestObject1>>())
            {
                var item1 = new TestObject1 {Name = "1233"};
                repos1.Store(item1);
                var item2 = new TestObject1 {Name = "1233"};
                //repos1.Store(item2);
                repos1.SaveChanges();

                var item = new TestObject
                    {
                        Name = "123",
                        Refrence = new Refrence<TestObject1>
                            {
                                Object = item1
                            },
                        Refrence1 = new Refrence<TestObject1>(repos1)
                            {                                 
                                Object = item2
                            }
                    };
                repos.Store(item);
                repos.SaveChanges();
                refId = item1.Id;

                realId = item.Id;
                realGuid = item.Guid;
                Assert.IsNotNullOrEmpty(realId);
            }
            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            {
                TestObject item = repos.Find(realGuid);

                var clone = item;
                Assert.NotNull(repos.Find(clone.Guid));
                var item2 = repos.Find(new[]{realGuid});
                TestObject item1 = repos.Find(realId + "123123123");
                Assert.NotNull(item);
                //Assert.NotNull(item);
                //item.Refrence.Repository = repos1;
                Assert.NotNull(item.Refrence.Repository);
                Assert.NotNull(item.Refrence.Object);
                Assert.AreEqual(item.Refrence.Object.Id, refId);
                Assert.Null(item1);

                repos.Delete(item);
                repos.SaveChanges();
            }

            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            using (var repos1 = _core.Resolve<IRepositoryWithGuid<TestObject1>>())
            {
                TestObject item = repos.Find(realId);
                Assert.Null(item);

                List<TestObject1> items = repos1.Query.ToList();
                Assert.IsTrue(items.Count > 0);
            }

            var lst = new List<TestObject>();
            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            {
                for (int i = 0; i < 10; i++)
                {
                    lst.Add(new TestObject {Name = i.ToString()});
                }
                repos.StoreBulk(lst);
                repos.SaveChanges();
            }

            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            {
                List<TestObject> res = repos.Find(lst.Select(i => i.Id)).ToList();
                Assert.AreEqual(res.Count, lst.Count);
            }
        }

        [Test]
        public void ReposCreationTest()
        {
            using (var repos = _core.Resolve<IRepositoryWithGuid<TestObject>>())
            {
                Assert.NotNull(repos);
            }
        }

        [Test]
        public void BulkOperationsTest()
        {
            var items = new List<TestObject3>();
            for (int i = 0; i < 505050; i++)
            {
                items.Add(new TestObject3
                    {
                        Name = "Tst" + i,
                        Guid = Guid.NewGuid(),
                        P = i
                    });
            }

            using (var repo = _core.Resolve<IRepository<TestObject3>>())
            {
                repo.StoreBulk(items);
            }

            using (var repo = _core.Resolve<IRepository<TestObject3>>())
            {
                Assert.AreEqual(items.Count, repo.GetAll().Count());
            }
        }
    }
}