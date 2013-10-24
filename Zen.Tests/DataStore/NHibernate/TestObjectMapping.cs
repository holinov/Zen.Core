using Zen.Tests.DataStore.DataModel;

namespace Zen.Tests.DataStore.NHibernate
{
    public class TestObjectMapping :FluentNHibernate.Mapping.ClassMap<TestObject>
    {
        public TestObjectMapping()
        {
            Id(o => o.Id);
            Map(o => o.Name);
        }
    }
}