using EveryDaily.Core.Settings;
using EveryDaily.Domain.Documents;
using Microsoft.Extensions.Options;

namespace EveryDaily.Persistence.BaseRepositories;

public class MongoDocContext(IOptions<MongoDbSettings> options) 
    : MongoDocContextFactory(options)
{
    // Asagidaki gibi bir property olusturarak her bir model icin bir DocSet olusturabiliriz.
    public DocSet<TestModel> TestModels => Create<TestModel>();
}