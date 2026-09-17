namespace sims_stix_ingest.DTOs;

using System.Text.Json.Serialization;

public class AggregatorDomainObjectDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("source_format")]
    public string SourceFormat { get; set; } = "stix";
}