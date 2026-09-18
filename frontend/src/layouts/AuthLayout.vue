<script setup lang="ts"></script>

<template>
  <div class="auth-layout">
    <div class="auth-container">
      <slot></slot>
    </div>
  </div>
</template>

<style scoped>
.auth-layout {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: radial-gradient(circle at center, var(--surface-color) 0%, var(--bg-color) 100%);
  padding: 1rem;
}

.auth-container {
  width: 100%;
  display: flex;
  justify-content: center;
}
</style>
