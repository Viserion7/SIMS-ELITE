const runtimeConfig = typeof window !== 'undefined' ? window.APP_CONFIG ?? {} : {}

export type ApiService = 'identity' | 'aggregator' | 'incidentManager' | 'stix'

export const identityUrl =
  runtimeConfig.identityUrl || import.meta.env.VITE_IDENTITY_URL || '/api/identity'

export const aggregatorUrl =
  runtimeConfig.aggregatorUrl || import.meta.env.VITE_AGGREGATOR_URL || '/api/aggregator'

export const incidentManagerUrl =
  runtimeConfig.incidentManagerUrl || import.meta.env.VITE_INCIDENT_MANAGER_URL || '/api/incident-manager'

export const stixUrl =
  runtimeConfig.stixUrl || import.meta.env.VITE_STIX_URL || '/api/stix'

export const getBaseUrl = (service: ApiService): string => {
  switch (service) {
    case 'identity':
      return identityUrl
    case 'aggregator':
      return aggregatorUrl
    case 'incidentManager':
      return incidentManagerUrl
    case 'stix':
      return stixUrl
  }
}

export const getApiUrl = (endpoint: string, service: ApiService = 'aggregator'): string => {
  const baseUrl = getBaseUrl(service)
  const path = endpoint.startsWith('/') ? endpoint : `/${endpoint}`

  return `${baseUrl.replace(/\/$/, '')}${path}`
}

export const getIdentityApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'identity')

export const getAggregatorApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'aggregator')

export const getIncidentManagerApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'incidentManager')

export const getStixApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'stix')

export default {
  identityUrl,
  aggregatorUrl,
  incidentManagerUrl,
  stixUrl,
  getBaseUrl,
  getApiUrl,
  getIdentityApiUrl,
  getAggregatorApiUrl,
  getIncidentManagerApiUrl,
  getStixApiUrl,
}