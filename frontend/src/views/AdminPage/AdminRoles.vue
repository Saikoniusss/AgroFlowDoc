<template>
  <div class="admin-page">
    <h2>Роли пользователей</h2>

    <!-- Таблица ролей -->
    <div class="role-actions">
      <input v-model="newRole.name" placeholder="Название роли" />
      <input v-model="newRole.description" placeholder="Описание" />
      <button @click="createRole">➕ Добавить</button>
    </div>

    <table class="role-table">
      <thead>
        <tr>
          <th>Роль</th>
          <th>Описание</th>
          <th>Пользователей</th>
          <th>Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="r in roles" :key="r.id">
          <td>{{ r.name }}</td>
          <td>{{ r.description }}</td>
          <td>{{ r.usersCount }}</td>
          <td>
            <button @click="openRoleEditor(r)">👥 Пользователи</button>
            <button @click="editRole(r)">✏️</button>
            <button @click="deleteRole(r.id)">🗑️</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Диалог редактирования роли -->
    <div v-if="editDialog" class="dialog">
      <div class="dialog-content">
        <h3>Редактировать роль</h3>
        <label>Название</label>
        <input v-model="editRoleData.name" />
        <label>Описание</label>
        <input v-model="editRoleData.description" />
        <div class="dialog-actions">
          <button @click="updateRole">💾 Сохранить</button>
          <button @click="closeDialogs">Отмена</button>
        </div>
      </div>
    </div>

    <!-- Диалог назначения пользователей -->
    <div v-if="userDialog" class="dialog">
      <div class="dialog-content large">
        <h3>Назначить пользователей для роли "{{ selectedRole?.name }}"</h3>

        <div class="user-list">
          <div v-for="u in users" :key="u.id" class="user-item">
            <input
              type="checkbox"
              :id="u.id"
              :value="u.id"
              v-model="selectedUserIds"
            />
            <label :for="u.id">
              {{ u.displayName }} ({{ u.username }})
              <small v-if="u.roles.length">[{{ u.roles.join(', ') }}]</small>
            </label>
          </div>
        </div>

        <div class="dialog-actions">
          <button @click="saveUsersForRole">💾 Сохранить</button>
          <button @click="closeDialogs">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import http from '@/api/http';

const roles = ref([]);
const users = ref([]);
const newRole = ref({ name: '', description: '' });
const selectedRole = ref(null);
const selectedUserIds = ref([]);
const editDialog = ref(false);
const userDialog = ref(false);
const editRoleData = ref({});

const loadRoles = async () => {
  const res = await http.get('/admin/roles');
  roles.value = res.data;
};

const loadUsers = async () => {
  const res = await http.get('/admin/users');
  users.value = res.data;
};

onMounted(() => {
  loadRoles();
  loadUsers();
});

// 🟩 Добавление роли
const createRole = async () => {
  if (!newRole.value.name.trim()) return;
  await http.post('/admin/roles', newRole.value);
  newRole.value = { name: '', description: '' };
  await loadRoles();
};

// 🟦 Удаление роли
const deleteRole = async (id) => {
  if (!confirm('Удалить роль?')) return;
  await http.delete(`/admin/roles/${id}`);
  await loadRoles();
};

// 🟨 Открыть редактирование
const editRole = (role) => {
  editRoleData.value = { ...role };
  editDialog.value = true;
};

const updateRole = async () => {
  await http.put(`/admin/roles/${editRoleData.value.id}`, editRoleData.value);
  await loadRoles();
  editDialog.value = false;
};

// 🟪 Открыть пользователей
const openRoleEditor = async (role) => {
  selectedRole.value = role;
  userDialog.value = true;

  // загружаем пользователей
  const res = await http.get('/admin/users');
  users.value = res.data;

  // отмечаем тех, кто уже имеет роль
  selectedUserIds.value = users.value
    .filter(u => u.roles.includes(role.name))
    .map(u => u.id);
};

// 🟫 Сохранить выбранных пользователей для роли
const saveUsersForRole = async () => {
  const roleName = selectedRole.value.name;

  // 1. Удаляем роль у всех пользователей, кто не отмечен
  const toRemove = users.value.filter(u =>
    u.roles.includes(roleName) && !selectedUserIds.value.includes(u.id)
  );
  for (const user of toRemove) {
    await http.post('/admin/deactivate-role', { userId: user.id, roleName });
  }

  // 2. Добавляем роль тем, кто выбран
  for (const id of selectedUserIds.value) {
    await http.post('/admin/assign-role', { userId: id, roleName });
  }

  await loadUsers();
  await loadRoles();
  userDialog.value = false;
};

// 🔘 Закрыть все диалоги
const closeDialogs = () => {
  editDialog.value = false;
  userDialog.value = false;
};
</script>

<style scoped>
.admin-page {
  padding: 2rem;
}

.role-actions {
  display: flex;
  gap: 10px;
  margin-bottom: 1rem;
}

.role-table {
  width: 100%;
  border-collapse: collapse;
}

.role-table th,
.role-table td {
  padding: 0.5rem;
  border-bottom: 1px solid #ddd;
}

button {
  margin-right: 0.3rem;
  cursor: pointer;
}

.dialog {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.3);
  display: flex;
  justify-content: center;
  align-items: center;
}

.dialog-content {
  background: white;
  padding: 1rem;
  border-radius: 10px;
  width: 400px;
}

.dialog-content.large {
  width: 600px;
  max-height: 80vh;
  overflow: auto;
}

.user-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-top: 1rem;
}

.user-item {
  padding: 4px;
  border-bottom: 1px solid #eee;
}

.dialog-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 1rem;
}
</style>