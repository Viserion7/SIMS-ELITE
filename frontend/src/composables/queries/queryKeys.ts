export const queryKeys = {
  auth: {
    all: ['auth'] as const,
    me: () => [...queryKeys.auth.all, 'me'] as const,
  },
  users: {
    all: ['users'] as const,
    list: () => [...queryKeys.users.all, 'list'] as const,
    detail: (id: number) => [...queryKeys.users.all, 'detail', id] as const,
    toNotify: () => [...queryKeys.users.all, 'toNotify'] as const,
  },
  categories: {
    all: ['categories'] as const,
    list: () => [...queryKeys.categories.all, 'list'] as const,
  },
  incidents: {
    all: ['incidents'] as const,
    list: (page: number, count: number) =>
      [...queryKeys.incidents.all, 'list', { page, count }] as const,
    detail: (id: string) => [...queryKeys.incidents.all, 'detail', id] as const,
    relationships: (incidentId: string) =>
      [...queryKeys.incidents.all, 'relationships', incidentId] as const,
  },
  relationships: {
    all: ['relationships'] as const,
    list: (page: number, count: number) =>
      [...queryKeys.relationships.all, 'list', { page, count }] as const,
    detail: (id: string) => [...queryKeys.relationships.all, 'detail', id] as const,
  },
} as const
