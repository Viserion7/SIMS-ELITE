import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

export function useMyCategories() {
  const authStore = useAuthStore()

  const myCategories = computed<Set<string>>(() => {
    const categories = new Set<string>()
    if (!authStore.userDetails?.levels) return categories

    for (const level of authStore.userDetails.levels) {
      if (level.categorys) {
        for (const category of level.categorys) {
          categories.add(category.name)
        }
      }
    }
    return categories
  })

  return myCategories
}
