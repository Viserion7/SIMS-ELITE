namespace sims_stix_ingest.Controllers;

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using DTOs;
using System.Net.Http.Json;

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

            if (!bsonDoc.Contains("objects"))
            {
                return BadRequest("Invalid STIX document: 'objects' are required.");
            }

            var objectsArray = bsonDoc["objects"].AsBsonArray;

            if (objectsArray.Contains("id") || objectsArray.Contains("type"))
            {
                return BadRequest("Invalid STIX document: 'objects' must contain at least one 'id' and 'type'.");
            }

            string bundleId = bsonDoc["id"].AsString;

            await _gateway.UpsertAsync(bundleId, bsonDoc);
            _logger.LogInformation("Successfully stored STIX bundle {BundleId} in MongoDB.", bundleId);

            List<ExtractedDomainObjectDto> rawDomainObjects = await _gateway.ExtractDomainObjectsAsync(bundleId);
            _logger.LogInformation("Extracted {Count} objects from bundle {BundleId}.", rawDomainObjects.Count,
                bundleId);

            var outgoingObjects = rawDomainObjects.Select(obj => new AggregatorDomainObjectDto
            {
                Id = StripLeadingPrefix(obj.Id), 
                Modified = obj.Modified,
                Type = obj.Type,
                Name = obj.Name,
                Description = obj.Description,
                SourceFormat = "stix"
            }).ToList();
            
            string aggregatorBaseUrl = _configuration["AggregatorSettings:BaseUrl"] ?? "http://aggregator:8080";
            string aggregatorTargetUrl = $"{aggregatorBaseUrl}/api/v1/Incidents";

            int successCount = 0;
            foreach (var domainObjectDto in outgoingObjects)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"{aggregatorTargetUrl}", domainObjectDto);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Aggregator returned status {Status} for object {Id}.", response.StatusCode, domainObjectDto.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send object {Id} to Aggregator service.", domainObjectDto.Id);
                }
            }

            return Ok(new
            {
                message = "STIX bundle stored and processed successfully.",
                bundleId = bundleId,
                totalExtracted = outgoingObjects.Count,
                forwardedToAggregator = successCount
            });
        }
        catch (FormatException ex)
        {
            return BadRequest($"Malformed JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing STIX bundle.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // Aggregator uses id as key, therefore strip leading prefix to turn id into valid uuid
    private static string StripLeadingPrefix(string stixId)
    {
        if (string.IsNullOrWhiteSpace(stixId)) return string.Empty;

        int delimiterIndex = stixId.IndexOf("--", StringComparison.Ordinal);
        return delimiterIndex >= 0 ? stixId[(delimiterIndex + 2)..] : stixId;
    }
}