<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Вход в систему</h2>
      <form @submit.prevent="onSubmit">
        <div class="form-group">
          <label>Имя пользователя</label>
          <input v-model="username" type="text" required />
        </div>

        <div class="form-group">
          <label>Пароль</label>
          <input v-model="password" type="password" required />
        </div>

        <button :disabled="auth.loading" type="submit">
          {{ auth.loading ? 'Входим...' : 'Войти' }}
        </button>

        <p v-if="auth.error" class="error">{{ auth.error }}</p>
        <p class="note">
          Нет аккаунта?
          <router-link to="/register">Зарегистрироваться</router-link>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/store/auth';

const router = useRouter();
const auth = useAuthStore();
const username = ref('');
const password = ref('');

const onSubmit = async () => {
  await auth.login(username.value, password.value);
  if (auth.user) {
    console.log(auth.user)
    window.location.href = "/documents"
  }
};
</script>

<style scoped>
.auth-page {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: #eef2f3;
}
.auth-card {
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  width: 350px;
}
h2 {
  text-align: center;
  margin-bottom: 1rem;
}
.form-group {
  margin-bottom: 1rem;
}
input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #ccc;
  border-radius: 0.5rem;
}
button {
  width: 100%;
  padding: 0.7rem;
  background-color: #42b883;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: bold;
  cursor: pointer;
}
button:disabled {
  background-color: #a1a1a1;
}
.error {
  color: red;
  text-align: center;
  margin-top: 0.5rem;
}
.note {
  text-align: center;
  margin-top: 1rem;
}
</style>