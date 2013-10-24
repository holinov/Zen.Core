using Zen.DataStore;

namespace Zen.Tests.DataStore.DataModel
{
    public class TestObject : HasGuidId
    {
        public virtual string Name { get; set; }
        public virtual Refrence<TestObject1> Refrence { get; set; }
        public virtual Refrence<TestObject1> Refrence1 { get; set; }
    }
}