<template>
  <div class="page">
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
        <h4 class="mt-4">Файлы вложения</h4>

        <input type="file" multiple @change="onFileSelected" />

        <ul>
          <li v-for="file in selectedFiles" :key="file.name">
            📄 {{ file.name }} ({{ (file.size/1024/1024).toFixed(2) }} MB)
          </li>
        </ul>
      </div>

      <div class="actions">
        <Button label="Отмена" class="p-button-secondary" @click="router.push('/documents')" />
        <Button label="Сохранить как черновик" class="p-button-warning" @click="saveDraft" />
        <Button label="Утвердить" class="p-button-success" @click="submit" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import documentApi from '@/api/documentApi'

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
  const { data } = await documentApi.getProcessDetails(id)
  template.value = data
  fields.value =  data.template.fields

  fields.value.forEach(f => {
    model.value[f.name] = ''
  })
})

const uploadAllFiles = async (documentId) => {
  for (const file of selectedFiles.value) {
    await documentApi.uploadFile(documentId, file)
  }
}

const saveDraft = async () => {
  await documentApi.createDocument({
    processId: route.params.processId,
    title: title.value,
    fieldsJson: JSON.stringify(model.value),
    submit: false
  })
  if (selectedFiles.value.length > 0)
    await uploadAllFiles(documentId)
  router.push('/documents')
}

const submit = async () => {
  await documentApi.createDocument({
    processId: route.params.processId,
    title: title.value,
    fieldsJson: JSON.stringify(model.value),
    submit: true
  })
  if (selectedFiles.value.length > 0)
    await uploadAllFiles(documentId)
  router.push('/documents')
}
</script>
