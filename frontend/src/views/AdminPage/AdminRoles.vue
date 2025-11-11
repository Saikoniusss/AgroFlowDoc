<script setup>
import { ref, onMounted } from 'vue';
import http from '@/api/http';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';

const roles = ref([]);
const users = ref([]);
const newRole = ref({ name: '', description: '' });
const selectedRole = ref(null);
const selectedUserIds = ref([]);
const editDialog = ref(false);
const userDialog = ref(false);
const editRoleData = ref({});
const deleteDialog = ref(false);

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
const deleteRole = async () => {
  await http.delete(`/admin/roles/${selectedRole.value.id}`);
  await loadRoles();
  selectedRole.value = null;
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

const openDeleteDialog = (role) => {
  selectedRole.value = role;
  deleteDialog.value = true;
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
  selectedRole.value = null;
};

// 🔘 Закрыть все диалоги
const closeDialogs = () => {
  editDialog.value = false;
  userDialog.value = false;
  deleteDialog.value = false;
  selectedRole.value = null;
};
</script>

<template>
  <DataTable :value="roles">
    <template #header>
      <h2>Роли пользователей</h2>
      <div class="role-actions" style="display: flex; gap: 0.5rem; align-items: center;">
        <InputText v-model="newRole.name" size="small" placeholder="Название" />
        <InputText v-model="newRole.description" size="small" placeholder="Описание" />
        <Button @click="createRole" size="small">➕ Добавить</Button>
      </div>
    </template>
    <Column field="name" header="Роль" />
    <Column field="description" header="Описание" />
    <Column field="usersCount" header="Пользователей" />
    <Column header="Действия">
      <template #body="{ data }" style="">
        <div style="flex-grow: 1; display: flex; gap: 0.5rem;">
          <Button @click="openRoleEditor(data)" size="small" severity="info" variant="text">👥 Пользователи</Button>
          <Button @click="editRole(data)" size="small" severity="info" variant="text">✏️ Редактировать</Button>
          <Button @click="openDeleteDialog(data)" size="small" severity="danger" variant="text">🗑️ Удалить</Button>
        </div>
      </template>
    </Column>
  </DataTable>

  <Dialog v-model:visible="editDialog" header="Редактировать роль">
    <div class="flex flex-col gap-3 mb-3">
      <label for="username">Название</label>
      <InputText id="username" v-model="editRoleData.name" aria-describedby="username-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="description">Описание</label>
        <InputText id="description" v-model="editRoleData.description" aria-describedby="description-help" fluid />
    </div>
    <Button @click="updateRole" size="small" severity="success" variant="text">💾 Сохранить</Button>
    <Button @click="closeDialogs" size="small" severity="secondary" variant="text">Отмена</Button>
  </Dialog>

  <Dialog v-model:visible="userDialog" :style="{ width: '50vw' }">
    <template #header>
      <h3>Назначить пользователей для роли "{{ selectedRole?.name }}"</h3>
    </template>
      <div class="user-list" style="max-height: 400px; overflow-y: auto;">
        <div v-for="u in users" :key="u.id">
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
    <template #footer>
      <div class="dialog-actions" style="flex-grow: 1; display: flex; gap: 0.5rem;">
        <Button @click="saveUsersForRole" size="small" severity="success" variant="text">💾 Сохранить</Button>
        <Button @click="closeDialogs" size="small" severity="secondary" variant="text">Отмена</Button>
      </div>
    </template>
  </Dialog>

  <Dialog v-model:visible="deleteDialog" header="Подтвердите удаление">
    <p>Вы уверены, что хотите удалить эту роль?</p>
    <div class="dialog-actions" style="flex-grow: 1; display: flex; gap: 0.5rem;">
      <Button @click="deleteRole(); closeDialogs()" size="small" severity="danger" variant="text">🗑️ Удалить</Button>
      <Button @click="closeDialogs" size="small" severity="secondary" variant="text">Отмена</Button>
    </div>
  </Dialog>

</template>

<style scoped>
</style>
