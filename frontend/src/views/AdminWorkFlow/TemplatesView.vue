<template>
  <div class="templates-page grid">
    <!-- Список шаблонов -->
    <div class="col-3 p-3 surface-100 border-right-1 border-gray-300">
      <h3>📄 Шаблоны</h3>
      <div class="flex justify-content-between mb-3">
        <Button label="Добавить" icon="pi pi-plus" @click="showCreateDialog = true" />
      </div>

      <ul class="list-none m-0 p-0">
        <li
          v-for="t in templates"
          :key="t.id"
          class="p-2 border-round cursor-pointer hover:surface-200"
          :class="{ 'bg-green-50': selectedTemplate?.id === t.id }"
          @click="selectTemplate(t)"
        >
          <div class="font-bold">{{ t.name }}</div>
          <div class="text-sm text-500">{{ t.code }}</div>
        </li>
      </ul>
    </div>

    <!-- Правая часть -->
    <div class="col p-4">
      <div v-if="!selectedTemplate">
        <p class="text-600">Выберите шаблон для редактирования</p>
      </div>

      <div v-else>
        <div class="flex justify-content-between align-items-center mb-3">
          <h3>{{ selectedTemplate.name }}</h3>
          <Button
            icon="pi pi-pencil"
            label="Редактировать шаблон"
            text
            @click="openEditTemplate(selectedTemplate)"
          />
        </div>

        <!-- Таблица полей -->
        <h4 class="mb-2">Поля шаблона</h4>

        <draggable
          v-model="localFields"
          item-key="id"
          handle=".drag-handle"
          animation="200"
          ghost-class="drag-ghost"
          chosen-class="drag-chosen"
          @end="saveFieldOrder"
          class="drag-table"
        >
          <template #item="{ element, index }">
            <div class="drag-row flex align-items-center justify-content-between p-2 border-bottom-1 border-200">
              <div class="flex align-items-center gap-2">
                <i class="pi pi-bars drag-handle text-600 cursor-move"></i>
                <span class="font-semibold">{{ index + 1 }}. {{ element.label }}</span>
                <small class="text-500">({{ element.fieldType }})</small>
                <i v-if="element.isRequired" class="pi pi-check text-green-600"></i>
              </div>
              <div class="flex gap-1">
                <Button icon="pi pi-pencil" text rounded @click="editField(element)" />
                <Button icon="pi pi-trash" text rounded severity="danger" @click="deleteField(element)" />
              </div>
            </div>
          </template>
        </draggable>

        <div class="mt-3">
          <Button label="Добавить поле" icon="pi pi-plus" @click="addField" />
        </div>

        <!-- Предпросмотр -->
        <h4>👁️ Предпросмотр шаблона</h4>
        <div v-if="sortedFields.length" class="preview border-round p-3 surface-50">
          <div v-for="field in sortedFields" :key="field.id" class="mb-3">
            <label class="block font-semibold mb-1">
              {{ field.label }}
              <span v-if="field.isRequired" class="text-red-500">*</span>
            </label>

            <!-- динамические элементы -->
            <template v-if="field.fieldType === 'text'">
              <InputText v-model="previewData[field.name]" class="w-full" />
            </template>

            <template v-else-if="field.fieldType === 'number'">
              <InputNumber v-model="previewData[field.name]" class="w-full" />
            </template>

            <template v-else-if="field.fieldType === 'date'">
              <Calendar v-model="previewData[field.name]" class="w-full" />
            </template>

            <template v-else-if="field.fieldType === 'select'">
              <Dropdown
                v-model="previewData[field.name]"
                :options="parseOptions(field.optionsJson)"
                optionLabel="label"
                optionValue="value"
                placeholder="Выберите..."
                class="w-full"
              />
            </template>
          </div>
        </div>
        <div v-else class="text-600">Нет полей для отображения</div>
      </div>
    </div>

    <!-- Диалог создания шаблона -->
    <Dialog v-model:visible="showCreateDialog" header="Создать шаблон" modal>
      <div class="p-fluid">
        <div class="p-field">
          <label>Название</label>
          <InputText v-model="newTemplate.name" />
        </div>
        <div class="p-field">
          <label>Код</label>
          <InputText v-model="newTemplate.code" />
        </div>
        <div class="p-field">
          <label>Описание</label>
          <Textarea v-model="newTemplate.description" rows="3" />
        </div>
      </div>
      <template #footer>
        <Button label="Отмена" text @click="showCreateDialog = false" />
        <Button label="Создать" icon="pi pi-check" @click="createTemplate" />
      </template>
    </Dialog>

    <!-- Диалог редактирования шаблона -->
    <Dialog v-model:visible="showEditTemplateDialog" header="Редактировать шаблон" modal>
      <div class="p-fluid">
        <div class="p-field">
          <label>Название</label>
          <InputText v-model="editingTemplate.name" />
        </div>
        <div class="p-field">
          <label>Код</label>
          <InputText v-model="editingTemplate.code" />
        </div>
        <div class="p-field">
          <label>Описание</label>
          <Textarea v-model="editingTemplate.description" rows="3" />
        </div>
      </div>
      <template #footer>
        <Button label="Отмена" text @click="showEditTemplateDialog = false" />
        <Button label="Сохранить" icon="pi pi-check" @click="saveTemplate" />
      </template>
    </Dialog>

    <!-- Диалог редактирования поля -->
    <Dialog v-model:visible="showFieldDialog" :header="fieldDialogTitle" modal :style="{ width: '600px' }">
      <div class="p-fluid">
        <div class="p-field">
          <label>Имя (name)</label>
          <InputText v-model="editingField.name" />
        </div>
        <div class="p-field">
          <label>Заголовок (label)</label>
          <InputText v-model="editingField.label" />
        </div>

        <div class="p-formgrid grid">
          <div class="field col">
            <label>Тип</label>
            <Dropdown v-model="editingField.fieldType" :options="fieldTypes" optionLabel="label" optionValue="value" />
          </div>
          <div class="field col">
            <label>Порядок</label>
            <InputNumber v-model="editingField.order" :min="1" />
          </div>
        </div>

        <div class="p-field-checkbox">
          <Checkbox v-model="editingField.isRequired" binary />
          <label>Обязательное</label>
        </div>

        <div v-if="editingField.fieldType === 'select'">
          <label>Варианты (JSON)</label>
          <Textarea v-model="editingField.optionsJson" rows="3" />
        </div>
      </div>
      <template #footer>
        <Button label="Отмена" text @click="showFieldDialog = false" />
        <Button label="Сохранить" icon="pi pi-check" @click="saveField" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch  } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Dropdown from 'primevue/dropdown'
import Checkbox from 'primevue/checkbox'
import Divider from 'primevue/divider'
import Dialog from 'primevue/dialog'
import Calendar from 'primevue/calendar'
import { useToast } from 'primevue/usetoast'
import adminWorkflowApi from '@/api/adminWorkflowApi'
import draggable from 'vuedraggable'

const toast = useToast()

const templates = ref([])
const selectedTemplate = ref(null)
const loading = ref(false)

const showCreateDialog = ref(false)
const showEditTemplateDialog = ref(false)
const showFieldDialog = ref(false)

const fieldDialogTitle = ref('')
const editingTemplate = ref({})
const editingField = ref({})
const newTemplate = ref({ name: '', code: '', description: '' })
const previewData = ref({})
// локальная копия, чтобы не ломать selectedTemplate.fields напрямую
const localFields = ref([])

const fieldTypes = [
  { label: 'Текст', value: 'text' },
  { label: 'Число', value: 'number' },
  { label: 'Дата', value: 'date' },
  { label: 'Список', value: 'select' }
]

const loadTemplates = async () => {
  loading.value = true
  try {
    const { data } = await adminWorkflowApi.getTemplates()
    templates.value = data
  } finally {
    loading.value = false
  }
}

const selectTemplate = (t) => {
  selectedTemplate.value = t
  previewData.value = {}
}

const sortedFields = computed(() =>
  (selectedTemplate.value?.fields || []).sort((a, b) => a.order - b.order)
)

const createTemplate = async () => {
  await adminWorkflowApi.createTemplate(newTemplate.value)
  toast.add({ severity: 'success', summary: 'Шаблон создан' })
  showCreateDialog.value = false
  await loadTemplates()
}

const openEditTemplate = (template) => {
  editingTemplate.value = { ...template }
  showEditTemplateDialog.value = true
}

const saveTemplate = async () => {
  await adminWorkflowApi.updateTemplate(editingTemplate.value.id, editingTemplate.value)
  toast.add({ severity: 'success', summary: 'Шаблон обновлён' })
  showEditTemplateDialog.value = false
  await loadTemplates()
  selectedTemplate.value = templates.value.find(x => x.id === editingTemplate.value.id)
}

const addField = () => {
  if (!selectedTemplate.value) return
  editingField.value = {
    name: '',
    label: '',
    fieldType: 'text',
    order: (selectedTemplate.value.fields?.length || 0) + 1,
    isRequired: false,
    optionsJson: '[]'
  }
  fieldDialogTitle.value = 'Добавить поле'
  showFieldDialog.value = true
}

const editField = (f) => {
  editingField.value = { ...f }
  fieldDialogTitle.value = 'Редактировать поле'
  showFieldDialog.value = true
}

const deleteField = async (f) => {
  await adminWorkflowApi.deleteField(selectedTemplate.value.id, f.id)
  toast.add({ severity: 'warn', summary: 'Поле удалено' })
  await loadTemplates()
  selectedTemplate.value = templates.value.find(x => x.id === selectedTemplate.value.id)
}

const saveField = async () => {
  const f = editingField.value
  if (f.id) {
    await adminWorkflowApi.updateField(selectedTemplate.value.id, f.id, f)
    toast.add({ severity: 'success', summary: 'Поле обновлено' })
  } else {
    await adminWorkflowApi.addField(selectedTemplate.value.id, f)
    toast.add({ severity: 'success', summary: 'Поле добавлено' })
  }
  showFieldDialog.value = false
  await loadTemplates()
  selectedTemplate.value = templates.value.find(x => x.id === selectedTemplate.value.id)
}

const parseOptions = (json) => {
  try {
    const parsed = JSON.parse(json || '[]')
    return parsed.map(opt => typeof opt === 'string' ? { label: opt, value: opt } : opt)
  } catch {
    return []
  }
}

// когда выбираешь шаблон — копируем его поля
watch(selectedTemplate, (tpl) => {
  if (tpl?.fields) {
    localFields.value = [...tpl.fields].sort((a, b) => a.order - b.order)
  } else {
    localFields.value = []
  }
})

// пересохраняем порядок после перетаскивания
const saveFieldOrder = async () => {
  if (!selectedTemplate.value) return
  // перенумеровываем
  localFields.value.forEach((f, idx) => (f.order = idx + 1))
  try {
    await adminWorkflowApi.updateFieldOrder(selectedTemplate.value.id, localFields.value)
    toast.add({ severity: 'success', summary: 'Порядок обновлён' })
    // обновляем в основном объекте
    selectedTemplate.value.fields = [...localFields.value]
  } catch (e) {
    console.error(e)
    toast.add({ severity: 'error', summary: 'Ошибка при сохранении порядка' })
  }
}

onMounted(loadTemplates)
</script>

<style scoped>
.templates-page {
  height: calc(100vh - 100px);
}
.preview input, .preview select {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 6px;
}
.preview {
  background: #fafafa;
}
.drag-row {
  transition: background 0.2s, transform 0.2s;
}
.drag-row:hover {
  background: #f7f7f7;
}
.drag-handle {
  cursor: grab;
}
.drag-ghost {
  background: #e9f7ef;
  opacity: 0.8;
}
.drag-chosen {
  background: #cde8ff;
}
.drag-table {
  border: 1px solid #ddd;
  border-radius: 6px;
  background: #fff;
}
</style>