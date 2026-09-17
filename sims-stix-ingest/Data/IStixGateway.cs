namespace sims_stix_ingest;

using MongoDB.Bson;
using DTOs;

public interface IStixGateway
{
    Task UpsertAsync(string bundleId, BsonDocument bundle);
    
    Task<List<ExtractedDomainObjectDto>> ExtractDomainObjectsAsync(string bundleId);
}