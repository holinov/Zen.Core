using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zen.DataStore.EntityFramework
{
    public interface IDbContextFactory
    {
        DbContext Create();
    }
}
