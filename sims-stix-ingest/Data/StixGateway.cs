namespace sims_stix_ingest.Data;

using MongoDB.Bson;
using MongoDB.Driver;
using DTOs;

public class StixGateway : IStixGateway
{
    private readonly IMongoCollection<BsonDocument> _collection;
    
    public StixGateway(IMongoClient mongoClient, IConfiguration config)
    {
        var dbName = config["MongoDbSettings:DatabaseName"] ?? "stix_db";
        var colName = config["MongoDbSettings:CollectionName"] ?? "stix_bundles";

        var database = mongoClient.GetDatabase(dbName);
        _collection = database.GetCollection<BsonDocument>(colName);
    }
    
    // upsert is up[date] (if exists) and [in]sert (if it doesnt)
    public async Task UpsertAsync(string bundleId, BsonDocument document)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", bundleId);
        document["_id"] = bundleId;
        
        await _collection.ReplaceOneAsync(
            filter, 
            document, 
            new ReplaceOptions { IsUpsert = true }
        );
    }
    
    // Aggregator requires individual Domain Objects, therefore split bundle into list of individual Domain Objects
    // query only for necessary properties to save resources
    // return empty list if no match
    public async Task<List<ExtractedDomainObjectDto>> ExtractDomainObjectsAsync(string bundleId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", bundleId);

        var projection = Builders<BsonDocument>.Projection
            .Include("objects.id")
            .Include("objects.created")
            .Include("objects.type")
            .Include("objects.name")
            .Include("objects.description")
            .Include("objects.source_ref")
            .Include("objects.target_ref")
            .Exclude("_id"); 

        var queryResult = await _collection
            .Find(filter)
            .Project(projection)
            .SingleOrDefaultAsync();

        var extractedList = new List<ExtractedDomainObjectDto>();
        
        if (queryResult == null || !queryResult.Contains("objects") || !queryResult["objects"].IsBsonArray)
        {
            return extractedList;
        }
        
        foreach (var item in queryResult["objects"].AsBsonArray)
        {
            if (item is BsonDocument obj)
            {
                extractedList.Add(new ExtractedDomainObjectDto 
                {
                    Id = obj.Contains("id") ? obj["id"].AsString : "N/A",
                    Created = obj.Contains("created") ? obj["created"].AsString : "N/A",
                    Name = obj.Contains("name") && !obj["name"].IsBsonNull ? obj["name"].AsString : "N/A",
                    Type = obj.Contains("type") ? obj["type"].AsString : "N/A",
                    Description = (obj.Contains("description") && !obj["description"].IsBsonNull) ? obj["description"].AsString : "N/A",
                    
                    SourceRef = obj.Contains("source_ref") ? obj["source_ref"].AsString : null,
                    TargetRef = obj.Contains("target_ref") ? obj["target_ref"].AsString : null
                });
            }
        }

        return extractedList;
    }
}