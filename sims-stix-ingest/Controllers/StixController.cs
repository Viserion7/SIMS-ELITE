namespace sims_stix_ingest.Controllers;

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using DTOs;
using System.Net.Http.Json;
using System.Net;

[Route("api/v1/[controller]")]
[ApiController]
public class StixController : ControllerBase
{
    private readonly IStixGateway _gateway;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StixController> _logger;

    public StixController(
        IStixGateway gateway,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<StixController> logger)
    {
        _gateway = gateway;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
    }

    // bundle needs 'id' to be processable and non-empty 'objects' to be usable data
    // objects in 'objects' need id to be processable by Aggregator and type to be usable data
    // read Aggregator config for flexible deployment
    [HttpPut]
    public async Task<IActionResult> IngestStixBundle([FromBody] JsonElement rawJson)
    {
        try
        {
            string jsonText = rawJson.GetRawText();
            var bsonDoc = BsonDocument.Parse(jsonText);

            if (!bsonDoc.Contains("id") || !bsonDoc.Contains("type"))
            {
                return BadRequest("Invalid STIX document: 'id' and 'type' are required.");
            }

            if (bsonDoc["type"].AsString != "bundle")
            {
                return BadRequest("Invalid STIX document: 'type' must be 'bundle'.");
            }

            if (!bsonDoc.Contains("objects")|| !bsonDoc["objects"].IsBsonArray)
            {
                return BadRequest("Invalid STIX document: 'objects' array is required.");
            }

            var objectsArray = bsonDoc["objects"].AsBsonArray;

            if (objectsArray.Count == 0)
            {
                return BadRequest("Invalid STIX document: 'objects' array cannot be empty.");
            }

            foreach (var item in objectsArray)
            {
                if (!item.IsBsonDocument)
                {
                    return BadRequest("Invalid STIX document: all items inside 'objects' must be valid objects.");
                }

                var domObj = item.AsBsonDocument;

                // Check if this specific object has both 'id' and 'type'
                if (!domObj.Contains("id") || !domObj.Contains("type"))
                {
                    return BadRequest("Invalid STIX document: every object inside 'objects' must contain an 'id' and 'type'.");
                }
            }

            string bundleId = bsonDoc["id"].AsString;

            await _gateway.UpsertAsync(bundleId, bsonDoc);
            _logger.LogInformation("Successfully stored STIX bundle '{BundleId}' in MongoDB.", bundleId);

            List<ExtractedDomainObjectDto> rawDomainObjects = await _gateway.ExtractDomainObjectsAsync(bundleId);
            _logger.LogInformation("Extracted {Count} objects from bundle '{BundleId}'.",
                rawDomainObjects.Count,
                bundleId);
            
            var regularObjects = new List<ExtractedDomainObjectDto>();
            var relationshipObjects = new List<ExtractedDomainObjectDto>();

            foreach (var obj in rawDomainObjects)
            {
                if (obj.Type == "relationship")
                {
                    relationshipObjects.Add(obj);
                }
                else
                {
                    regularObjects.Add(obj);
                }
            }
            _logger.LogInformation("Partitioned bundle '{BundleId}' into " + 
                                   "{ObjectCount} Domain Objects and {RelationshipCount} relationships.",
                bundleId,
                regularObjects.Count,
                relationshipObjects.Count);
            
            string aggregatorBaseUrl = _configuration["AggregatorSettings:BaseUrl"] ?? "http://aggregator:8080";
            string aggregatorTargetUrl = $"{aggregatorBaseUrl}/api/v1/Incidents";

            int successCount = 0;
            int conflictCount = 0;
            foreach (var obj in regularObjects)
            {
                Guid safeGuid = GetSafeGuid(obj.Id, contextField: "Domain Object ID");
                
                DateTime safeCreatedAt;
                if (DateTime.TryParse(obj.Created, out DateTime dateTime))
                {
                    safeCreatedAt = dateTime.ToUniversalTime();
                }
                else
                {
                    safeCreatedAt = DateTime.UtcNow;
                    _logger.LogWarning("Domain Object '{ObjectId}' has invalid or missing timestamp '{RawDate}'. " + 
                                       "Falling back to UTC Now: '{FallbackDate}'.",
                        obj.Id,
                        obj.Created,
                        safeCreatedAt);
                }
                
                var outgoingObject = new AggregatorDomainObjectDto
                {
                    Id = safeGuid,
                    CreatedAt = safeCreatedAt,
                    Type = obj.Type,
                    Name = obj.Name,
                    Desc = obj.Description,
                    SourceFormat = "stix"
                };
                
                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"{aggregatorTargetUrl}", outgoingObject);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        successCount++;
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        conflictCount++;
                        _logger.LogInformation("Domain Object '{ObjectId}' already exists in Aggregator (409 Conflict).",
                            outgoingObject.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Aggregator returned status '{Status}' for Domain Object '{ObjectId}'.",
                            response.StatusCode,
                            outgoingObject.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send Domain Object '{ObjectId}' to Aggregator service.",
                        outgoingObject.Id);
                }
            }
            
            string aggregatorRelationshipTargetUrl = $"{aggregatorBaseUrl}/api/v1/Relationships";
            int relationshipsCreated = 0;
            int relationshipConflicts = 0;
            foreach (var obj in relationshipObjects)
            {
                if (string.IsNullOrEmpty(obj.SourceRef) || string.IsNullOrEmpty(obj.TargetRef))
                {
                    _logger.LogWarning("Skipping relationship '{RelationshipId}': " + 
                                       "missing source_ref ('{SourceRef}') or target_ref ('{TargetRef}').",
                        obj.Id,
                        obj.SourceRef ?? "null",
                        obj.TargetRef ?? "null");
                    continue;
                }
                
                Guid relGuid  = GetSafeGuid(obj.Id, contextField: "Relationship ID");
                Guid fromGuid = GetSafeGuid(obj.SourceRef, contextField: "Relationship source_ref ID");
                Guid toGuid   = GetSafeGuid(obj.TargetRef, contextField: "Relationship target_ref ID");

                var outgoingRelationship = new AggregatorRelationshipDto()
                {
                    Id = relGuid,
                    From = fromGuid,
                    To = toGuid
                };
                
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(aggregatorRelationshipTargetUrl, outgoingRelationship);
            
                    if (response.IsSuccessStatusCode)
                    {
                        relationshipsCreated++;
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        relationshipConflicts++;
                        _logger.LogInformation("Relationship '{RelationshipId}' already exists in Aggregator (409 Conflict).",
                            outgoingRelationship.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Aggregator rejected relationship '{RelationshipId}' " + 
                                           "(from {From} to {To}) with status {Status}.", 
                            outgoingRelationship.Id,
                            outgoingRelationship.From,
                            outgoingRelationship.To,
                            response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send relationship {RelationshipId} to Aggregator.",
                        outgoingRelationship.Id);
                }
            }
            _logger.LogInformation(
                "Completed processing bundle '{BundleId}'. " +
                "Domain Objects created: {IncCreated}, duplicates: {IncDup}, " +
                "Relationships created: {RelCreated}, duplicates: {RelDup}.",
                bundleId,
                successCount,
                conflictCount,
                relationshipsCreated,
                relationshipConflicts);

            return Ok(new
            {
                message = "STIX bundle stored and processed successfully.",
                bundleId = bundleId,
                totalExtracted = regularObjects.Count + relationshipObjects.Count,
                IncidentsCreated = successCount,
                IncidentsDuplicate = conflictCount,
                RelationshipsCreated = relationshipsCreated,
                RelationshipsDuplicate = relationshipConflicts
            });
        }
        catch (FormatException ex)
        {
            return BadRequest($"Malformed JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // Aggregator uses id as key, therefore strip leading prefix to turn id into valid uuid
    private Guid GetSafeGuid(string? stixId, string contextField = "Fallback Field Name")
    {
        if (string.IsNullOrWhiteSpace(stixId))
        {
            Guid fallback = Guid.NewGuid();
            _logger.LogWarning("Field '{FieldName}' was empty or null. Generated fallback UUID: '{FallbackGuid}'.",
                contextField,
                fallback);
            return fallback;
        }

        int delimiterIndex = stixId.IndexOf("--", StringComparison.Ordinal);
        string rawGuid = delimiterIndex >= 0 ? stixId[(delimiterIndex + 2)..] : stixId;

        if (Guid.TryParse(rawGuid, out Guid guid))
        {
            return guid;
        }

        Guid generatedGuid = Guid.NewGuid();
        _logger.LogWarning("Value '{RawGuid}' in field '{FieldName}' is not a valid GUID. Generated fallback GUID: '{FallbackGuid}'.", 
            rawGuid,
            contextField,
            generatedGuid);
        return generatedGuid;
    }
}