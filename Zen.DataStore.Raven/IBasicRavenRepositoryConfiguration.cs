using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zen.DataStore.Raven
{
    public interface IBasicRavenRepositoryConfiguration
    {
        bool WaitForStaleIndexes { get; set; }
    }
}
