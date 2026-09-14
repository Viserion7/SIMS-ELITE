import { getApiUrl, type ApiService } from '@/config'

export type ApiRequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
}

export const apiClient = async <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
  service: ApiService = 'aggregator',
): Promise<ResponseData | null> => {
  const { body, headers = {}, ...requestOptions } = options
  const requestHeaders = new Headers(headers)

  if (body !== undefined) {
    requestHeaders.set('Content-Type', 'application/json')
  }

  const response = await fetch(getApiUrl(endpoint, service), {
    ...requestOptions,
    headers: requestHeaders,
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  })

  if (!response.ok) {
    throw new Error(`API request failed: ${response.status} ${response.statusText}`)
  }

  if (response.status === 204) return null
  return response.json() as Promise<ResponseData>
}

export const identityApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'identity')

export const aggregatorApiClient = <ResponseData = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
) => apiClient<ResponseData>(endpoint, options, 'aggregator')