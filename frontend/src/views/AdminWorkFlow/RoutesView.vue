<template>
  <div class="routes-layout">
    <!-- ЛЕВАЯ ПАНЕЛЬ -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <h3>Маршруты</h3>
        <Button icon="pi pi-plus" label="Создать" @click="showCreateDialog = true" size="small" />
      </div>

      <ul class="route-list">
        <li
          v-for="route in routes"
          :key="route.id"
          :class="{ active: selectedRoute?.id === route.id }"
          @click="selectRoute(route)"
        >
          {{ route.name }}
        </li>
      </ul>
    </aside>

    <!-- ПРАВАЯ ПАНЕЛЬ -->
    <main class="main-panel">
      <div v-if="selectedRoute" class="route-details">
        <h2>{{ selectedRoute.name }}</h2>
        <p class="route-code">Код: {{ selectedRoute.code }}</p>

        <h3>Этапы согласования</h3>
        <draggable v-model="selectedRoute.steps" item-key="id" @end="onReorder(selectedRoute)">
          <template #item="{ element }">
            <div class="step-item">
              <div>
                <b>{{ element.stepOrder }}.</b> {{ element.stepName }}
                <small v-if="element.isParallel">(Параллельно)</small>
              </div>
              <div class="step-actions">
                <Button icon="pi pi-pencil" text rounded @click="openEditStep(selectedRoute, element)" />
                <Button icon="pi pi-trash" text rounded severity="danger" @click="deleteStep(selectedRoute, element)" />
              </div>
            </div>
          </template>
        </draggable>

        <div class="step-buttons">
          <Button icon="pi pi-plus" label="Добавить этап" @click="openAddStep(selectedRoute)" />
          <Button icon="pi pi-save" label="Сохранить порядок" severity="success" @click="onReorder(selectedRoute)" />
        </div>
      </div>

      <div v-else class="placeholder">
        <p>Выберите маршрут из списка слева или создайте новый.</p>
      </div>
    </main>

    <!-- Диалог создания маршрута -->
    <Dialog v-model:visible="showCreateDialog" header="Создать маршрут" modal :style="{ width: '450px' }">
      <div class="flex flex-col gap-3 mb-3">
        <label for="name" style="min-width: 80px">Название</label>
        <InputText id="name" v-model="newRoute.name" aria-describedby="name-help" fluid/>
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="code" style="min-width: 80px">Код</label>
        <InputText id="code" v-model="newRoute.code" aria-describedby="code-help" fluid/>
      </div>
      <div class="flex flex-col gap-3 mb-3">
        <label for="description" style="min-width: 80px">Описание</label>
        <InputText id="description" v-model="newRoute.description" aria-describedby="description-help" fluid />
      </div>
      <Button @click="createRoute" size="small" severity="success" variant="text">Создать</Button>
      <Button @click="showCreateDialog = false" size="small" severity="secondary" variant="text">Отмена</Button>
    </Dialog>

    <!-- Диалоги для шагов -->
    <Dialog v-model:visible="showStepDialog" header="Добавить этап" modal :style="{ width: '650px' }">
      <StepEditor
        :route="selectedRoute"
        :step="newStep"
        mode="create"
        @save="createStep"
        @cancel="showStepDialog = false"
      />
    </Dialog>

    <Dialog v-model:visible="showEditStepDialog" header="Редактировать этап" modal :style="{ width: '650px' }">
      <StepEditor
        :route="selectedRoute"
        :step="editingStep"
        mode="edit"
        @save="updateStep"
        @cancel="showEditStepDialog = false"
      />
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { useToast } from 'primevue/usetoast'
import draggable from 'vuedraggable'
import StepEditor from './components/StepEditor.vue'
import adminWorkflowApi from '@/api/adminWorkflowApi'
import http from '../../api/http'

const toast = useToast()
const routes = ref([])
const selectedRoute = ref(null)
const loading = ref(false)

const showCreateDialog = ref(false)
const showStepDialog = ref(false)
const showEditStepDialog = ref(false)

const newRoute = ref({ name: '', code: '', description: '' })
const newStep = ref({})
const editingStep = ref({})

const loadRoutes = async () => {
  loading.value = true
  try {
    const { data } = await http.get('/v1/admin/workflow/routes')
    routes.value = data
  } finally {
    loading.value = false
  }
}

const selectRoute = (route) => {
  selectedRoute.value = route
}

const createRoute = async () => {
  await http.post('/v1/admin/workflow/routes', newRoute.value)
  toast.add({ severity: 'success', summary: 'Маршрут создан' })
  showCreateDialog.value = false
  await loadRoutes()
}

const openAddStep = (route) => {
  newStep.value = {
    stepName: '',
    stepOrder: route.steps.length + 1,
    isParallel: false,
    minApprovals: 1,
    approversJson: '[]'
  }
  showStepDialog.value = true
}

const createStep = async (payload) => {
  await http.post(`/v1/admin/workflow/routes/${selectedRoute.value.id}/steps`, payload)
  toast.add({ severity: 'success', summary: 'Этап добавлен' })
  showStepDialog.value = false
  await loadRoutes()
}

const openEditStep = (route, step) => {
  editingStep.value = JSON.parse(JSON.stringify(step))
  showEditStepDialog.value = true
}

const updateStep = async (payload) => {
  await http.post(`/v1/admin/workflow/routes/${selectedRoute.value.id}/steps`, editingStep.value.id, payload)
  toast.add({ severity: 'success', summary: 'Этап обновлён' })
  showEditStepDialog.value = false
  await loadRoutes()
}

const deleteStep = async (route, step) => {
  if (confirm(`Удалить этап "${step.stepName}"?`)) {
    await http.delete(`/v1/admin/workflow/routes/${route.id}/steps/${step.id}`)
    toast.add({ severity: 'success', summary: 'Этап удалён' })
    await loadRoutes()
  }
}

const onReorder = async (route) => {
  route.steps.forEach((s, idx) => (s.stepOrder = idx + 1))
  const payload = route.steps.map(s => ({
    id: s.id,
    stepOrder: s.stepOrder
  }))
  try {
    await http.put(`/v1/admin/workflow/routes/${route.id}/steps/reorder`, payload)
    toast.add({ severity: 'success', summary: 'Порядок сохранён' })
  } catch (err) {
    console.error(err)
    toast.add({ severity: 'error', summary: 'Ошибка сохранения порядка' })
  }
}

onMounted(loadRoutes)
</script>

<style scoped>
.routes-layout {
  display: flex;
  height: calc(100vh - 100px);
}

.sidebar {
  width: 280px;
  border-right: 1px solid #dcdcdc;
  background: #fafafa;
  padding: 1rem;
  overflow-y: auto;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.route-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.route-list li {
  padding: 8px 10px;
  margin-bottom: 4px;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.2s;
}

.route-list li:hover {
  background: #eef6ff;
}

.route-list li.active {
  background: #42b883;
  color: white;
}

.main-panel {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}

.step-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 10px;
  margin-bottom: 4px;
  border: 1px solid #ddd;
  border-radius: 6px;
  background: #fff;
  cursor: grab;
}

.step-buttons {
  margin-top: 1rem;
  display: flex;
  gap: 1rem;
}

.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  color: gray;
  height: 100%;
}
</style>