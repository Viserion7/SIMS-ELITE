export interface StixObject {
  id: string
  type: string
  [key: string]: unknown
}

export interface StixBundle {
  id: string
  type: 'bundle'
  objects: StixObject[]
  [key: string]: unknown
}
