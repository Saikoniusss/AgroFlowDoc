<template>
    <Layout/>
</template>

<script setup>
import Layout from '@/layouts/Layout.vue';
import { useAuthStore } from '@/store/auth';
import { useRouter } from 'vue-router';

const auth = useAuthStore();
const router = useRouter();

const logout = async () => {
  await auth.logout();
  router.push('/login');
};
</script>

<style scoped>
.page {
  max-width: 800px;
  margin: 80px auto;
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
button {
  background: #ff5555;
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 0.5rem;
  cursor: pointer;
}
button:hover {
  background: #e04444;
}
</style>