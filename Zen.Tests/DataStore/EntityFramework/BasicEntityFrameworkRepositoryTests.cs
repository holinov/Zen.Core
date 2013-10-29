using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zen.DataStore.EntityFramework;

namespace Zen.Tests.DataStore.EntityFramework
{
    [TestFixture]
    public class BasicEntityFrameworkRepositoryTests
    {
        IDbContextFactory _contextFactory;

        public BasicEntityFrameworkRepositoryTests()
        {
            _contextFactory = new TestDbContextFactory();
        }

        [Test]
        public void TestCrud()
        {
            var repo = new BasicEntityFrameworkRepository<TestEntity>(_contextFactory);
            var newEntity1 = new TestEntity { Guid = Guid.NewGuid(), Name = "Name1" };
            var newEntity2 = new TestEntity { Guid = Guid.NewGuid(), Name = "Name2" };
            repo.Store(newEntity1);
            repo.Store(newEntity2);
            repo.SaveChanges();

            var entityFromDb = repo.Find(newEntity1.Id);

            Assert.AreEqual(newEntity1.Id, entityFromDb.Id);

            var allEntities = repo.Query.ToList();
            Assert.AreEqual(2, allEntities.Count());
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity1.Id));
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity2.Id));

            allEntities = repo.Find(new List<string> { newEntity1.Id, newEntity2.Id}).ToList();
            Assert.AreEqual(2, allEntities.Count());
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity1.Id));
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity2.Id));

            allEntities = repo.GetAll().ToList();
            Assert.AreEqual(2, allEntities.Count());
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity1.Id));
            Assert.IsTrue(allEntities.Any(e => e.Id == newEntity2.Id));

            repo.DeleteById(newEntity1.Id);
            allEntities = repo.GetAll().ToList();
            //Save changes has been not called yet
            Assert.AreEqual(2, allEntities.Count());

            repo.SaveChanges();
            allEntities = repo.GetAll().ToList();
            //Save changes has been not called yet
            Assert.AreEqual(1, allEntities.Count());
            
            repo.Delete(allEntities[0]);
            repo.SaveChanges();
            allEntities = repo.GetAll().ToList();
            Assert.IsEmpty(allEntities);
        }

        [Test]
        public void TestStoreBulk()
        {
            var repo = new BasicEntityFrameworkRepository<TestEntity>(_contextFactory);
            var newEntity1 = new TestEntity { Guid = Guid.NewGuid(), Name = "Name1" };
            var newEntity2 = new TestEntity { Guid = Guid.NewGuid(), Name = "Name2" };
            repo.StoreBulk(new List<TestEntity> { newEntity1, newEntity2 });

            var allEntities = repo.GetAll();
            Assert.AreEqual(2, allEntities.Count());
        }
    }
}
