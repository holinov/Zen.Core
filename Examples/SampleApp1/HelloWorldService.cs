using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleApp1;
using Zen;
using Zen.DataStore;
using Zen.Host;
using log4net;

namespace SampleApp1
{
    public class HelloWorldService : IHelloWorldService
    {
        private readonly IRepositoryWithGuid<UserQueryCount> _userRepository;

        public HelloWorldService(IRepositoryWithGuid<UserQueryCount> userRepository)
        {
            _userRepository = userRepository;
        }

        public string Hello(string name)
        {
            var userProfile = _userRepository.Query.FirstOrDefault(i => i.UserName == name);
            if (userProfile == null)
            {
                userProfile = new UserQueryCount()
                    {
                        Count = 1,
                        UserName = name
                    };
                _userRepository.Store(userProfile);
            }
            else
            {
                userProfile.Count++;
            }
            _userRepository.SaveChanges();

            return string.Format("{0}] Hello {1} count: {2}", DateTime.Now, name, userProfile.Count);
        }

        public string GetWebserviceName()
        {
            return GetType().Name;
        }
    }
}