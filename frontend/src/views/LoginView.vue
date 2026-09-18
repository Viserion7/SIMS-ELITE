<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useLoginMutation } from '@/composables/queries/useAuthQueries'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'

const router = useRouter()
const loginMutation = useLoginMutation()

const email = ref('')
const password = ref('')

const handleLogin = async () => {
  if (!email.value || !password.value) return
  
  try {
    await loginMutation.mutateAsync({ email: email.value, password: password.value })
    router.push('/incidents')
  } catch (error) {
    // Error wird über das Mutation-Objekt in der UI (Message) angezeigt
  }
}
</script>

<template>
  <div class="login-wrapper">
    <div class="login-card">
      <div class="login-header">
        <h1 class="logo-text">SIMS-ELITE</h1>
        <p class="login-subtitle">Security Incident Management</p>
      </div>

      <Message v-if="loginMutation.isError.value" severity="error" :closable="false" class="mb-4">
        Anmeldung fehlgeschlagen. Bitte Zugangsdaten prüfen.
      </Message>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="field">
          <label for="email">E-Mail</label>
          <InputText 
            id="email" 
            v-model="email" 
            type="email" 
            class="w-full" 
            placeholder="admin@local" 
            required 
            autofocus
          />
        </div>

        <div class="field">
          <label for="password">Passwort</label>
          <Password 
            id="password" 
            v-model="password" 
            :feedback="false" 
            toggleMask 
            class="w-full"
            inputClass="w-full"
            placeholder="••••••••" 
            required 
          />
        </div>

        <Button 
          type="submit" 
          label="Anmelden" 
          class="w-full submit-btn" 
          :loading="loginMutation.isPending.value" 
        />
      </form>

      <div class="login-footer">
        <RouterLink to="/upload" class="upload-link">
          STIX Datei hochladen &rarr;
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 1rem;
}

.login-card {
  width: 100%;
  max-width: 480px;
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  padding: 2.5rem;
}

:global(.dark-mode) .login-card {
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.5);
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-text {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--primary-color);
  letter-spacing: -0.5px;
  margin: 0;
}

.login-subtitle {
  color: var(--text-color-secondary);
  margin-top: 0.5rem;
  margin-bottom: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field label {
  font-weight: 500;
  color: var(--text-color);
}

/* Fix for primevue full width styling */
:deep(.p-password) {
  width: 100%;
}

.submit-btn {
  margin-top: 1rem;
}

.login-footer {
  margin-top: 2rem;
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border-color);
}

.upload-link {
  color: var(--primary-color);
  font-weight: 500;
  text-decoration: none;
  font-size: 0.875rem;
  transition: opacity 0.2s;
}

.upload-link:hover {
  opacity: 0.8;
}
</style>
