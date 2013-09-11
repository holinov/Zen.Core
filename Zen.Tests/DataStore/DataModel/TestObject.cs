using Zen.DataStore;

namespace Zen.Tests.DataStore.DataModel
{
    public class TestObject : HasGuidId
    {
        public string Name { get; set; }
        public Refrence<TestObject1> Refrence { get; set; }
        public Refrence<TestObject1> Refrence1 { get; set; }
    }
}