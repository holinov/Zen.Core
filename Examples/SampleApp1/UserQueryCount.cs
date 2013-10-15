using Zen.DataStore;

namespace SampleApp1
{
    public class UserQueryCount : HasGuidId
    {
        public string UserName { get; set; }
        public int Count { get; set; }
    }
}