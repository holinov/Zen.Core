using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zen.DataStore.Raven
{
    public class BasicRavenRepositoryConfiguration : IBasicRavenRepositoryConfiguration
    {
        public bool WaitForStaleIndexes { get; set; }
    }
}
