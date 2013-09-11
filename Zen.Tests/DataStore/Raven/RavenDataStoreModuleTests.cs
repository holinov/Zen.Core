using NUnit.Framework;
using Zen.DataStore;
using Zen.DataStore.Raven;
using Zen.Tests.DataStore.DataModel;

namespace Zen.Tests.DataStore.Raven
{
    [TestFixture]
    public class RavenDataStoreModuleTests
    {
        [Test]
        public void TestRavenDataStoreModule()
        {
            using (var core = AppCoreBuilder.Create()
                                            .AddModule<RavenDataStoreModule>()
                                            .AddModule<RavenRepositoriesModule>()
                                            .Build())
            {
                using (var rep = core.Resolve<IRepositoryWithGuid<TestObject>>())
                {
                    Assert.NotNull(rep);
                }
            }
        }
    }
}