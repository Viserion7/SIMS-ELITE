import { useQuery } from '@tanstack/vue-query'
import { aggregatorApiClient } from '@/api/client'

export function useExampleQuery() {
  return useQuery({
    queryKey: ['example'],
    queryFn: () => aggregatorApiClient('/api/example'),
    enabled: false,
  })
}