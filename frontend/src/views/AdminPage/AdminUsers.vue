<template>
  <div class="admin-page">
    <h2>Пользователи</h2>

    <table class="user-table">
      <thead>
        <tr>
          <th>Имя</th>
          <th>Логин</th>
          <th>Email</th>
          <th>Подтверждён</th>
          <th>Активен</th>
          <th>Роли</th>
          <th>Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in users" :key="u.id">
          <td>{{ u.displayName }}</td>
          <td>{{ u.username }}</td>
          <td>{{ u.email }}</td>
          <td>
            <span :class="['status', u.isApproved ? 'ok' : 'pending']">
              {{ u.isApproved ? 'Да' : 'Нет' }}
            </span>
          </td>
          <td>
            <span :class="['status', u.isActive ? 'ok' : 'inactive']">
              {{ u.isActive ? 'Да' : 'Нет' }}
            </span>
          </td>
          <td>
            <span v-for="r in u.roles" :key="r" class="role-chip">{{ r }}</span>
          </td>
          <td>
            <button v-if="!u.isApproved" @click="approveUser(u.id)">✅ Подтвердить</button>
            <button v-if="u.isActive" @click="deactivateUser(u.id)">🚫 Отключить</button>
            <button @click="openRoleDialog(u)">🎯 Назначить роль</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="showDialog" class="dialog">
      <div class="dialog-content">
        <h3>Назначить роль пользователю {{ selectedUser?.displayName }}</h3>
        <select v-model="selectedRole">
          <option v-for="r in roles" :key="r.id" :value="r.name">{{ r.name }}</option>
        </select>
        <div class="dialog-actions">
          <button @click="assignRole">Сохранить</button>
          <button @click="closeDialog">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import http from '@/api/http';

const users = ref([]);
const roles = ref([]);
const showDialog = ref(false);
const selectedUser = ref(null);
const selectedRole = ref(null);

const loadData = async () => {
  const [u, r] = await Promise.all([
    http.get('/admin/users'),
    http.get('/admin/roles')
  ]);
  users.value = u.data;
  roles.value = r.data;
};

onMounted(loadData);

const approveUser = async (id) => {
  await http.post(`/admin/approve/${id}`);
  await loadData();
};

const deactivateUser = async (id) => {
  await http.post(`/admin/deactivate/${id}`);
  await loadData();
};

const openRoleDialog = (user) => {
  selectedUser.value = user;
  selectedRole.value = null;
  showDialog.value = true;
};

const closeDialog = () => {
  showDialog.value = false;
};

const assignRole = async () => {
  await http.post('/admin/assign-role', {
    userId: selectedUser.value.id,
    roleName: selectedRole.value
  });
  showDialog.value = false;
  await loadData();
};
</script>

<style scoped>
.admin-page {
  padding: 2rem;
}
.user-table {
  width: 100%;
  border-collapse: collapse;
}
.user-table th, .user-table td {
  padding: 0.5rem;
  border-bottom: 1px solid #ddd;
  text-align: left;
}
.status.ok { color: green; }
.status.pending { color: orange; }
.status.inactive { color: red; }
.role-chip {
  background: #eef;
  border-radius: 8px;
  padding: 2px 6px;
  margin-right: 4px;
}
.dialog {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.3);
  display: flex;
  justify-content: center;
  align-items: center;
}
.dialog-content {
  background: #fff;
  padding: 1rem;
  border-radius: 8px;
  width: 300px;
}
.dialog-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 1rem;
}
</style>