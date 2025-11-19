<script setup>
import { ref, onMounted } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import InputText from 'primevue/inputtext';
import IconField from 'primevue/iconfield';
import InputIcon from 'primevue/inputicon';
import { FilterMatchMode } from '@primevue/core/api';
import Avatar from 'primevue/avatar';
import Button from 'primevue/button';
import http from '@/api/http';
import Dialog from 'primevue/dialog';
import Select from 'primevue/select';
import Card from 'primevue/card';

const users = ref([]);
const roles = ref([]);
const showDialog = ref(false);
const selectedUser = ref(null);
const selectedRole = ref(null);
const message = ref('');
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
});

const loadData = async () => {
  const [u, r] = await Promise.all([
    http.get('/users'),
    http.get('/users/roles')
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
  try {
    await http.post('/admin/assign-role', {
      userId: selectedUser.value.id,
      roleName: selectedRole.value
    })
  } catch (error) {
    console.log(error);
    message.value = error.response.data || 'Неверный запрос (400)'
    return;
  }
  showDialog.value = false;
  await loadData();
  
};
</script>

<template>
  <Card class="p-1 border-2">
    <template #content>
      <DataTable :value="users" v-model:filters="filters" ilterDisplay="row" size="small" paginator :rows="5" :globalFilterFields="['displayName', 'username', 'email']">
        <template #header>
          <h2 class="m-0">Управление пользователями</h2>
            <div class="flex justify-between">
              <IconField>
                <InputIcon>
                  <i class="pi pi-search" />
                </InputIcon>
                <InputText v-model="filters['global'].value" placeholder="Поиск" />
              </IconField>
            </div>
        </template>
        <Column field="displayName" header="Имя" sortable></Column>
        <Column field="username" header="Логин" sortable></Column>
        <Column field="photo" header="Фото">
          <template #body="slotProps">
            <Avatar :image="slotProps.data.photo" size="medium" shape="circle" :label="slotProps.data.displayName ? slotProps.data.displayName.charAt(0).toUpperCase() : ''"/>
          </template>
        </Column>
        <Column field="email" header="Email" sortable></Column>
        <Column field="isApproved" header="Подтверждён">
          <template #body="slotProps">
            <span :class="['status', slotProps.data.isApproved ? 'ok' : 'pending']">
              {{ slotProps.data.isApproved ? 'Да' : 'Нет' }}
            </span>
          </template>
        </Column>
        <Column field="isActive" header="Активен">
          <template #body="slotProps">
            <span :class="['status', slotProps.data.isActive ? 'ok' : 'inactive']">
              {{ slotProps.data.isActive ? 'Да' : 'Нет' }}
            </span>
          </template>
        </Column>
        <Column field="roles" header="Роли">
          <template #body="slotProps">
            <span v-for="r in slotProps.data.roles" :key="r" class="role-chip">{{ r.name }}</span>
          </template>
        </Column>
        <Column header="Действия">
          <template #body="slotProps">
            <Button v-if="!slotProps.data.isApproved" size="small" severity="success" variant="text" @click="approveUser(slotProps.data.id)">✅ Подтвердить</Button>
            <Button v-if="slotProps.data.isActive" size="small" severity="danger" variant="text" @click="deactivateUser(slotProps.data.id)">🚫 Отключить</Button>
            <Button v-if="!slotProps.data.isActive" size="small" severity="success" variant="text">✅ Включить</Button>
            <Button size="small" variant="text" severity="info" @click="openRoleDialog(slotProps.data)">🎯 Назначить роль</Button>
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
  <Dialog v-model:visible="showDialog" @hide="closeDialog">
    <template #header>
      Назначить роль пользователю {{ selectedUser ? selectedUser.displayName : '' }}
    </template>
      <div class="mb-4">
        <label for="role-select" class="block mb-2 font-medium">Выберите роль:</label>
        <Select id="role-select" v-model="selectedRole" class="w-full" :options="roles" optionLabel="name" optionValue="name" placeholder="Выберите роль" />
      </div>
      <div class="flex justify-end gap-2">
        <Button label="Отмена" severity="secondary" variant="text" size="small" @click="closeDialog" />
        <Button label="Назначить" :disabled="!selectedRole" variant="text" size="small" @click="assignRole" />
      </div>
    <template #footer>
      <p v-if="message" class="error-message text-red-500">{{ message }}</p>
    </template>
  </Dialog>
</template>

<style scoped>
</style>