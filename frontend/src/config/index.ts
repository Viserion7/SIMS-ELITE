const runtimeConfig = typeof window !== 'undefined' ? window.APP_CONFIG ?? {} : {}

export const identityUrl = runtimeConfig.identityUrl ?? ''
export const aggregatorUrl = runtimeConfig.aggregatorUrl ?? ''

export type ApiService = 'identity' | 'aggregator'

export const getApiUrl = (endpoint: string, service: ApiService = 'aggregator'): string => {
  const baseUrl = service === 'identity' ? identityUrl : aggregatorUrl
  const path = endpoint.startsWith('/') ? endpoint : `/${endpoint}`

  return `${baseUrl.replace(/\/$/, '')}${path}`
}

export const getIdentityApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'identity')

export const getAggregatorApiUrl = (endpoint: string): string =>
  getApiUrl(endpoint, 'aggregator')

export default {
  identityUrl,
  aggregatorUrl,
  getApiUrl,
  getIdentityApiUrl,
  getAggregatorApiUrl,
}