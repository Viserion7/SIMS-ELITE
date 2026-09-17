namespace sims_stix_ingest.DTOs;

public class ExtractedDomainObjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string? SourceRef { get; set; }
    public string? TargetRef { get; set; }
}