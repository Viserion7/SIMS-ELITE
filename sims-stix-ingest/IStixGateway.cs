namespace sims_stix_ingest;

using MongoDB.Bson;

public interface IStixGateway
{
    Task UpsertAsync(string bundleId, BsonDocument bundle);
    
    Task<List<DomainObject>> ExtractDomainObjectsAsync(string bundleId);
}