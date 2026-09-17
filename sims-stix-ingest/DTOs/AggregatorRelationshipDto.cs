namespace sims_stix_ingest.DTOs;

using System.Text.Json.Serialization;

public class AggregatorRelationshipDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("from")]
    public Guid From { get; set; }
    [JsonPropertyName("to")]
    public Guid To { get; set; }
}