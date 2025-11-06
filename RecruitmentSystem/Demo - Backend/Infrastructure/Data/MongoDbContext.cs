using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Core.Models;
using Infrastructure.Configuration;

namespace Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("jobs");
    }
}
