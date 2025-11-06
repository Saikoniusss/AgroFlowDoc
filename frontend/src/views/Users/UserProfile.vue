<template>
  <div class="page">
    <h2>👤 Профиль пользователя</h2>
    <p><strong>Имя:</strong> {{ auth.user?.displayName }}</p>
    <p><strong>Логин:</strong> {{ auth.user?.username }}</p>
    <p><strong>Email:</strong> {{ auth.user?.email }}</p>
    <p><strong>Телеграм:</strong> {{ auth.user?.telegramUsername || 'не привязан' }}</p>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/store/auth';
const auth = useAuthStore();
</script>

<style scoped>
.page {
  max-width: 600px;
  margin: 60px auto;
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
</style>