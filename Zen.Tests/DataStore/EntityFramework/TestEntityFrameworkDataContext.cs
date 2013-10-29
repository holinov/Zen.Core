using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zen.DataStore;

namespace Zen.Tests.DataStore.EntityFramework
{
    public class TestEntityFrameworkDataContext : DbContext
    {
        public TestEntityFrameworkDataContext()
        { 
        }

        public TestEntityFrameworkDataContext(DbConnection connection, bool contextOwnsConnection) 
            : base(connection, contextOwnsConnection)
        {
        }

        DbSet<TestEntity> TestEntities { get; set; }
    }

    public class TestEntity : HasGuidId
    {
        public string Name { get; set; }
    }
}
