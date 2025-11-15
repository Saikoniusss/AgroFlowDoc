<template>
  <div class="admin-page">
    <h2>⚙️ Процессы согласования</h2>

    <div class="actions">
      <Button label="Создать процесс" icon="pi pi-plus" @click="showCreateDialog = true"></Button>
    </div>

    <DataTable :value="processes" class="p-datatable-sm" :loading="loading">
      <Column field="name" header="Название" />
      <Column field="template.name" header="Шаблон" />
      <Column field="route.name" header="Маршрут" />
    </DataTable>

    <Dialog v-model:visible="showCreateDialog" header="Создать процесс" modal>
      <div class="flex flex-col gap-3 mb-3">
        <label for="name" style="min-width: 160px">Название</label>
        <InputText id="name" v-model="newProcess.name" aria-describedby="name-help" fluid/>
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="code" style="min-width: 160px">Код</label>
        <InputText id="code" v-model="newProcess.code" aria-describedby="code-help" fluid/>
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="description" style="min-width: 160px">Описание</label>
        <Textarea id="description" v-model="newProcess.description" aria-describedby="description-help" fluid />
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="templates" style="min-width: 160px">Шаблон документа</label>
        <Dropdown v-model="newProcess.documentTemplateId" :options="templates" optionLabel="name" optionValue="id" placeholder="Выберите шаблон" fluid/>
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="routes" style="min-width: 160px">Маршрут согласования</label>
        <Dropdown v-model="newProcess.workflowRouteId" :options="routes" optionLabel="name" optionValue="id" placeholder="Выберите шаблон" fluid/>
      </div>
      <Button @click="createProcess" size="small" severity="success" variant="text">Создать</Button>
      <Button @click="showCreateDialog = false" size="small" severity="secondary" variant="text">Отмена</Button>
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
import http from '../../api/http'

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
      http.get('/v1/admin/workflow/processes'),
      http.get('/v1/admin/workflow/templates'),
      http.get('/v1/admin/workflow/routes')
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
    await http.post('/v1/admin/workflow/processes', newProcess.value)
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