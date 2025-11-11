<template>
  <div class="admin-page">
    <h2>⚙️ Процессы согласования</h2>

    <div class="actions">
      <Button label="Создать процесс" icon="pi pi-plus" @click="showCreateDialog = true" />
    </div>

    <DataTable :value="processes" class="p-datatable-sm" :loading="loading">
      <Column field="name" header="Название" />
      <Column field="template.name" header="Шаблон" />
      <Column field="route.name" header="Маршрут" />
    </DataTable>

    <Dialog v-model:visible="showCreateDialog" header="Создать процесс" modal>
      <div class="p-fluid">
        <div class="p-field">
          <label>Название</label>
          <InputText v-model="newProcess.name" />
        </div>
        <div class="p-field">
          <label>Код</label>
          <InputText v-model="newProcess.code" />
        </div>
        <div class="p-field">
          <label>Описание</label>
          <Textarea v-model="newProcess.description" rows="2" />
        </div>
        <div class="p-field">
          <label>Шаблон документа</label>
          <Dropdown
            v-model="newProcess.documentTemplateId"
            :options="templates"
            optionLabel="name"
            optionValue="id"
            placeholder="Выберите шаблон"
          />
        </div>
        <div class="p-field">
          <label>Маршрут согласования</label>
          <Dropdown
            v-model="newProcess.workflowRouteId"
            :options="routes"
            optionLabel="name"
            optionValue="id"
            placeholder="Выберите маршрут"
          />
        </div>
      </div>
      <template #footer>
        <Button label="Отмена" text @click="showCreateDialog = false" />
        <Button label="Создать" icon="pi pi-check" @click="createProcess" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Dropdown from 'primevue/dropdown'
import { useToast } from 'primevue/usetoast'
import adminWorkflowApi from '@/api/adminWorkflowApi'

const toast = useToast()
const loading = ref(false)
const processes = ref([])
const templates = ref([])
const routes = ref([])
const showCreateDialog = ref(false)

const newProcess = ref({
  name: '',
  code: '',
  description: '',
  documentTemplateId: null,
  workflowRouteId: null
})

const loadAll = async () => {
  loading.value = true
  try {
    const [p, t, r] = await Promise.all([
      adminWorkflowApi.getProcesses(),
      adminWorkflowApi.getTemplates(),
      adminWorkflowApi.getRoutes()
    ])
    processes.value = p.data
    templates.value = t.data
    routes.value = r.data
  } finally {
    loading.value = false
  }
}

const createProcess = async () => {
  try {
    await adminWorkflowApi.createProcess(newProcess.value)
    toast.add({ severity: 'success', summary: 'Процесс создан' })
    showCreateDialog.value = false
    await loadAll()
  } catch {
    toast.add({ severity: 'error', summary: 'Ошибка создания процесса' })
  }
}

onMounted(loadAll)
</script>

<style scoped>
.admin-page {
  padding: 2rem;
}
.actions {
  margin-bottom: 1rem;
}
</style>