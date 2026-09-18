import { getApiUrl, type ApiService } from '@/config'

export type ApiRequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
  skipAuth?: boolean
}

const getStoredToken = (): string | null => {
  if (typeof window === 'undefined') return null
  return localStorage.getItem('sims_access_token')
}

export const apiClient = async <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
  service: ApiService = 'aggregator',
): Promise<ResponseData | null> => {
  const { body, headers = {}, skipAuth = false, ...requestOptions } = options
  const requestHeaders = new Headers(headers)

  if (body !== undefined && !requestHeaders.has('Content-Type')) {
    requestHeaders.set('Content-Type', 'application/json')
  }

  if (!skipAuth && !requestHeaders.has('Authorization')) {
    const token = getStoredToken()
    if (token) {
      requestHeaders.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(getApiUrl(endpoint, service), {
    ...requestOptions,
    headers: requestHeaders,
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  })

  if (!response.ok) {
    let errorMessage = `API request failed: ${response.status} ${response.statusText}`
    const errorText = await response.text().catch(() => '')
    if (errorText) {
      try {
        const errorJson = JSON.parse(errorText)
        errorMessage = errorJson.message || errorJson.title || errorText
      } catch {
        errorMessage = errorText
      }
    }
    throw new Error(errorMessage)
  }

  if (response.status === 204) return null as ResponseData

  const text = await response.text()
  if (!text || text.trim() === '') {
    return null as ResponseData
  }

  try {
    return JSON.parse(text) as ResponseData
  } catch {
    return text as unknown as ResponseData
  }
}

export const identityApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'identity')

export const aggregatorApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'aggregator')

export const incidentManagerApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'incidentManager')

export const stixApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'stix')
