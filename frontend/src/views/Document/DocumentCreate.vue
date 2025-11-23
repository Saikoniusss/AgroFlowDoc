<template>
  <Card class="p-1 border-2">
    <template #content>
    <h2>Создание документа</h2>

    <div class="card">
      <h3>{{ template?.name }}</h3>

      <div class="form">
        <label>Название документа</label>
        <InputText v-model="title" class="w-full" />

        <div v-for="f in fields" :key="f.id" class="form-group">
          <label>{{ f.label }}</label>

          <InputText v-if="f.fieldType === 'text'" v-model="model[f.name]" class="w-full" />
          <InputNumber v-if="f.fieldType === 'number'" v-model="model[f.name]" class="w-full" />
          <Calendar v-if="f.fieldType === 'date'" v-model="model[f.name]" class="w-full" />

          <Dropdown
            v-if="f.fieldType === 'select'"
            :options="JSON.parse(f.optionsJson || '[]')"
            v-model="model[f.name]"
            class="w-full"
          />
        </div>
        <!-- ФАЙЛЫ -->
          <div>
            <h4 class="mt-4">Файлы вложения</h4>

            <ul>
              <li v-for="(file, index) in selectedFiles" :key="file.name" class="flex items-center gap-2">
                📄 {{ file.name }} ({{ (file.size / 1024 / 1024).toFixed(2) }} MB)
                <button type="button" @click="removeFile(index)">❌</button>
              </li>
            </ul>

            <input type="file" multiple @change="onFileSelected" />
          </div>
      </div>

      <div class="actions">
        <Button label="Отмена" class="p-button-secondary" @click="router.push('/documents')" />
        <Button label="Сохранить как черновик" class="p-button-warning" @click="saveDraft" />
        <Button label="Утвердить" class="p-button-success" @click="submit" />
      </div>
    </div>

    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import http from '../../api/http'
import { Card } from 'primevue'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const processId = route.query.processId
const template = ref(null)
const fields = ref([])
const model = ref({})
const title = ref('')
const selectedFiles = ref([])

onMounted(async () => {
  const id = route.params.processId
  const { data } = await http.get(`/v1/documents/process/${id}`)
  template.value = data
  fields.value =  data.template.fields

  fields.value.forEach(f => {
    model.value[f.name] = ''
  })
})

// Добавление файлов
function onFileSelected(event) {
  const files = Array.from(event.target.files)
  selectedFiles.value.push(...files)
  event.target.value = null // сброс для возможности добавлять новые
}

// Удаление файла
function removeFile(index) {
  selectedFiles.value.splice(index, 1)
}

// Загрузка файлов на сервер
async function uploadFiles(documentId) {
  for (const file of selectedFiles.value) {
    const formData = new FormData()
    formData.append('file', file)
    try {
      await http.post(`/v1/documents/${documentId}/files/upload`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    } catch (err) {
      console.error('Ошибка загрузки файла', file.name, err)
      toast.add({ severity: 'error', summary: 'Ошибка', detail: `Не удалось загрузить файл ${file.name}` })
    }
  }
  selectedFiles.value = []
}

const saveDraft = async () => {
  const { data } = await http.post('/v1/documents/create', {
    processId: route.params.processId,
    title: title.value,
    fieldsJson: JSON.stringify(model.value),
    submit: false
  })
  const documentId = data.documentId
  if (!documentId) {
    toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Сервер не вернул ID документа' })
    return
  }
  if (selectedFiles.value.length > 0)
    await uploadFiles(documentId)
  toast.add({ severity: 'success', summary: 'Успех', detail: 'Документ сохранен как черновик' })
  router.push('/documents')
}

const submit = async () => {
  const  { data } = await http.post('/v1/documents/create', {
    processId: route.params.processId,
    title: title.value,
    fieldsJson: JSON.stringify(model.value),
    submit: true
  })
  const documentId = data.documentId
  if (!documentId) {
    toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Сервер не вернул ID документа' })
    return
  }
  if (selectedFiles.value.length > 0)
    await uploadFiles(documentId)
  toast.add({ severity: 'success', summary: 'Успех', detail: 'Документ отправлен на согласование' })
  router.push('/documents')
}
</script>
