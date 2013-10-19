using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Zen.DataStore.Mongo
{
    public class MongoDbContext
    {
        private readonly MongoClient _client;
        private readonly MongoServer _server;
        private readonly MongoDatabase _database;

        public MongoDbContext(string connectionString = "mongodb://localhost", string databaseName = "ZenDatabase")
        {
            _client = new MongoClient(connectionString);
            _server = _client.GetServer();
            _database = _server.GetDatabase(databaseName);
        }

        public MongoDatabase Database
        {
            get { return _database; }
        }
    }
}
