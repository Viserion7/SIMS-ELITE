export interface StixIngestResponse {
  message: string
  bundleId: string
  totalExtracted: number
  incidentsCreated?: number
  incidentsDuplicate?: number
  relationshipsCreated?: number
  relationshipsDuplicate?: number
  IncidentsCreated?: number
  IncidentsDuplicate?: number
  RelationshipsCreated?: number
  RelationshipsDuplicate?: number
}
