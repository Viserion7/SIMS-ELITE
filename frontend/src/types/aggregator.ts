export interface Incident {
  id: string
  created_at: string
  db_created_at: string
  is_deleted: boolean
  deleted_by?: number | null
  source_format: string
  type: string
  name: string
  desc: string
}

export interface CreateIncidentDto {
  id?: string
  created_at?: string
  source_format?: string
  type?: string
  name?: string
  desc?: string
}

export interface Relationship {
  id: string
  idFrom: string
  idTo: string
}

export interface PaginationParams {
  page: number
  count: number
}
