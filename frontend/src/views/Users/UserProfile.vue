<template>
  <div class="profile-page">
    <h2>Мой профиль</h2>

    <form @submit.prevent="updateProfile">
      <label>Имя:</label>
      <input v-model="profile.displayName" type="text" />

      <label>Email:</label>
      <input v-model="profile.email" type="email" />

      <label>Telegram username:</label>
      <input v-model="profile.telegramUsername" type="text" placeholder="@username" />

      <div class="tg-status">
        <template v-if="profile.telegramChatId">
          ✅ Telegram привязан (Chat ID: {{ profile.telegramChatId }})
        </template>
        <template v-else>
          🔗 Напиши сообщение боту
          <a href="https://t.me/AgroRequestSenderBot" target="_blank">@AgroRequestSenderBot</a>,
          после чего система автоматически свяжет аккаунт.
        </template>
      </div>

      <button type="submit">💾 Сохранить</button>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import http from '@/api/http';

const profile = ref({});

const loadProfile = async () => {
  const res = await http.get('/profile/me');
  profile.value = res.data;
};

onMounted(loadProfile);

const updateProfile = async () => {
  await http.put('/profile/update', {
    displayName: profile.value.displayName,
    email: profile.value.email,
    telegramUsername: profile.value.telegramUsername,
  });
  alert('Профиль обновлён');
  await loadProfile();
};
</script>

<style scoped>
.profile-page {
  padding: 2rem;
  max-width: 480px;
  margin: 0 auto;
}
form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
input {
  padding: 0.6rem;
  border: 1px solid #ccc;
  border-radius: 6px;
}
button {
  background: #42b883;
  color: white;
  padding: 0.7rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
.tg-status {
  font-size: 0.9rem;
  color: #333;
}
</style>