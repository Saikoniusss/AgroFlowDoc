<template>
  <Card class="p-1 border-2">
    <template #content>
      <div class="grid">

        <div class="w-3">
          <div class="sidebar-header">
            <h3 class="flex justify-content-between align-items-center mb-3">Шаблоны <Button icon="pi pi-plus" @click="showCreateDialog = true" size="small" /></h3>
          </div>
          <Listbox :options="templates" optionLabel="name" @change="selectTemplate">
            <template #optiongroup="slotProps">
              <div class="flex items-center">
                <div>{{ slotProps.option.label }}</div>
              </div>
            </template>
          </Listbox>
        </div>

        <div class="w-9">
          <div v-if="!selectedTemplate">
            <p class="text-600">Выберите шаблон для редактирования</p>
          </div>
          <div v-else>
            <div class="">
              <h2>{{ selectedTemplate.name }}</h2>
              <Button
                icon="pi pi-pencil"
                label="Редактировать шаблон"
                text
                @click="openEditTemplate(selectedTemplate)"
              />
            </div>

            <!-- Таблица полей -->
            <Card>
              <template #header>
                <h4 class="mb-2">Поля шаблона</h4>
              </template>
              <template #content>
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
              </template>
            </Card>
            <!-- Предпросмотр -->
            <Card>
              <template #header>
                <h4 class="mb-2">👁️ Предпросмотр шаблона</h4>
              </template>
              <template #content>
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
              </template>
            </Card>
          </div>
        </div>
      </div>
    </template>
  </Card>
  <!-- Диалог создания шаблона -->
  <Dialog v-model:visible="showCreateDialog" header="Создать шаблон" modal>
    <div class="flex flex-col gap-3 mb-3">
      <label for="name" style="min-width: 80px">Название</label>
      <InputText id="name" v-model="newTemplate.name" aria-describedby="name-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="code" style="min-width: 80px">Код</label>
      <InputText id="code" v-model="newTemplate.code" aria-describedby="code-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="description" style="min-width: 80px">Описание</label>
      <Textarea id="description" v-model="newTemplate.description" aria-describedby="description-help" fluid cols="50" rows="5" />
    </div>
    <Button @click="createTemplate" size="small" severity="success" variant="text">Создать</Button>
    <Button @click="showCreateDialog = false" size="small" severity="secondary" variant="text">Отмена</Button>
  </Dialog>

  <!-- Диалог редактирования шаблона -->
  <Dialog v-model:visible="showEditTemplateDialog" header="Редактировать шаблон" modal>
    <div class="flex flex-col gap-3 mb-3">
      <label for="name" style="min-width: 80px">Название</label>
      <InputText id="name" v-model="editingTemplate.name" aria-describedby="name-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="code" style="min-width: 80px">Код</label>
      <InputText id="code" v-model="editingTemplate.code" aria-describedby="code-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="description" style="min-width: 80px">Описание</label>
      <InputText id="description" v-model="editingTemplate.description" aria-describedby="description-help" fluid />
    </div>
    <Button @click="saveTemplate" size="small" severity="success" variant="text">Создать</Button>
    <Button @click="showEditTemplateDialog = false" size="small" severity="secondary" variant="text">Отмена</Button>
  </Dialog>

  <!-- Диалог редактирования поля -->
  <Dialog v-model:visible="showFieldDialog" :header="fieldDialogTitle" modal :style="{ width: '600px' }">
    <div class="flex flex-col gap-3 mb-3">
      <label for="name" style="min-width: 160px">Имя (name)</label>
      <InputText id="name" v-model="editingField.name" aria-describedby="name-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="label" style="min-width: 160px">Заголовок (label)</label>
      <InputText id="label" v-model="editingField.label" aria-describedby="label-help" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="fieldType" style="min-width: 160px">Тип</label>
      <Dropdown v-model="editingField.fieldType" :options="fieldTypes" optionLabel="label" optionValue="value" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="order" style="min-width: 160px">Порядок</label>
      <InputNumber v-model="editingField.order" :min="1" fluid/>
    </div>
    <div class="flex flex-col gap-3 mb-3">
      <label for="isRequired" style="min-width: 160px">Обязательное</label>
      <Checkbox v-model="editingField.isRequired" binary />
    </div>
    <div class="flex flex-col gap-3 mb-3" v-if="editingField.fieldType === 'select'">
      <label for="optionsJson" style="min-width: 160px">Варианты (JSON)</label>
      <Textarea v-model="editingField.optionsJson" rows="3" />
    </div>
    <template #footer>
      <Button label="Отмена" text @click="showFieldDialog = false" />
      <Button label="Сохранить" icon="pi pi-check" @click="saveField" />
    </template>
  </Dialog>
</template>

<script setup>
import { ref, computed, onMounted, watch  } from 'vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Dropdown from 'primevue/dropdown'
import Checkbox from 'primevue/checkbox'
import Dialog from 'primevue/dialog'
import Calendar from 'primevue/calendar'
import { useToast } from 'primevue/usetoast'
import draggable from 'vuedraggable'
import http from '../../api/http'
import { Card } from 'primevue'
import Listbox from 'primevue/listbox';

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
    const { data } = await http.get('/v1/admin/workflow/templates')
    templates.value = data
  } finally {
    loading.value = false
  }
}

const selectTemplate = (t) => {
  console.log(t)
  selectedTemplate.value = t.value
  previewData.value = {}
}

const sortedFields = computed(() =>
  (selectedTemplate.value?.fields || []).sort((a, b) => a.order - b.order)
)

const createTemplate = async () => {
  await http.post('/v1/admin/workflow/templates', newTemplate.value)
  toast.add({ severity: 'success', summary: 'Шаблон создан' })
  showCreateDialog.value = false
  await loadTemplates()
}

const openEditTemplate = (template) => {
  editingTemplate.value = { ...template }
  showEditTemplateDialog.value = true
}

const saveTemplate = async () => {
  await http.put('/v1/admin/workflow/templates/' + editingTemplate.value.id, editingTemplate.value)
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
  await http.delete(`/v1/admin/workflow/templates/${selectedTemplate.value.id}/fields/${f.id}`)
  toast.add({ severity: 'warn', summary: 'Поле удалено' })
  await loadTemplates()
  selectedTemplate.value = templates.value.find(x => x.id === selectedTemplate.value.id)
}

const saveField = async () => {
  const f = editingField.value
  if (f.id) {
    await http.put(`/v1/admin/workflow/templates/${selectedTemplate.value.id}/fields/${f.id}`, f)
    toast.add({ severity: 'success', summary: 'Поле обновлено' })
  } else {
    await http.post(`/v1/admin/workflow/templates/${selectedTemplate.value.id}/fields`, f)
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
    await http.put(`/v1/admin/workflow/templates/${selectedTemplate.value.id}/fields/reorder`, localFields.value)
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

</style>