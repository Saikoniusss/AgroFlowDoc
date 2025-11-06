<template>
  <div class="auth-page">
    <div class="auth-card">
      <h2>Регистрация</h2>
      <form @submit.prevent="onSubmit">
        <div class="form-group">
          <label>Имя пользователя</label>
          <input v-model="form.username" type="text" required />
        </div>

        <div class="form-group">
          <label>Пароль</label>
          <input v-model="form.password" type="password" required />
        </div>

        <div class="form-group">
          <label>Имя отображаемое</label>
          <input v-model="form.displayName" type="text" required />
        </div>

        <div class="form-group">
          <label>Email</label>
          <input v-model="form.email" type="email" required />
        </div>

        <button :disabled="auth.loading" type="submit">
          {{ auth.loading ? 'Регистрируем...' : 'Зарегистрироваться' }}
        </button>

        <p v-if="auth.error" class="error">{{ auth.error }}</p>
        <p v-if="success" class="success">{{ success }}</p>
        <p class="note">
          Уже есть аккаунт?
          <router-link to="/login">Войти</router-link>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/store/auth';

const auth = useAuthStore();
const form = ref({
  username: '',
  password: '',
  displayName: '',
  email: '',
});
const success = ref('');

const onSubmit = async () => {
  try {
    const res = await auth.register(form.value);
    success.value = res.message;
  } catch {}
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
.success {
  color: green;
  text-align: center;
  margin-top: 0.5rem;
}
.note {
  text-align: center;
  margin-top: 1rem;
}
</style>