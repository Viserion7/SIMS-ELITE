<script setup lang="ts">
defineProps<{
  icon: string
  title: string
  description: string
}>()
</script>

<template>
  <div class="empty-state">
    <i :class="['pi', icon, 'empty-icon']"></i>
    <h3 class="empty-title">{{ title }}</h3>
    <p class="empty-desc">{{ description }}</p>
    <div class="empty-actions" v-if="$slots.default">
      <slot></slot>
    </div>
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
}

.empty-icon {
  font-size: 3rem;
  color: var(--text-color-secondary);
  margin-bottom: 1.5rem;
  opacity: 0.5;
}

.empty-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-color);
  margin: 0 0 0.5rem 0;
}

.empty-desc {
  color: var(--text-color-secondary);
  margin: 0 0 1.5rem 0;
  max-width: 400px;
}

.empty-actions {
  display: flex;
  gap: 1rem;
}
</style>
