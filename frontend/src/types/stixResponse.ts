export interface StixIngestResponse {
  message: string
  bundleId: string
  totalExtracted: number
  IncidentsCreated: number
  IncidentsDuplicate: number
  RelationshipsCreated: number
  RelationshipsDuplicate: number
}
