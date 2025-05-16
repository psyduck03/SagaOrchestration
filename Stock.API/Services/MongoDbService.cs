using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDbService
{
    private IMongoDatabase _database;

    public MongoDbService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
        _database = client.GetDatabase("Orchestration");
    }

    public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}