<template>
  <div class="page">
    <h2>👤 Подтверждение пользователей</h2>

    <table v-if="users.length">
      <thead>
        <tr>
          <th>Имя</th>
          <th>Email</th>
          <th>Статус</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in users" :key="u.id">
          <td>{{ u.displayName }}</td>
          <td>{{ u.email }}</td>
          <td>{{ u.isApproved ? '✅ Подтверждён' : '⏳ Ожидает' }}</td>
          <td v-if="!u.isApproved">
            <button @click="approve(u.id)">Подтвердить</button>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-else>Нет пользователей для подтверждения.</p>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import http from '@/api/http';

const users = ref([]);

onMounted(async () => {
  const res = await http.get('/users');
  users.value = res.data;
});

const approve = async (id) => {
  await http.post(`/users/${id}/approve`);
  const u = users.value.find(x => x.id === id);
  if (u) u.isApproved = true;
};
</script>

<style scoped>
.page {
  max-width: 900px;
  margin: 80px auto;
  background: white;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}
th, td {
  padding: 0.75rem;
  border-bottom: 1px solid #eee;
  text-align: left;
}
button {
  padding: 0.4rem 0.8rem;
  background: #42b883;
  color: white;
  border: none;
  border-radius: 0.4rem;
  cursor: pointer;
}
button:hover {
  background: #3aa876;
}
</style>