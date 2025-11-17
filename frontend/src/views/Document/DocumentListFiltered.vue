<template>
  <div class="page">
    <h2>{{ pageTitle }}</h2>

    <DataTable :value="documents" :loading="loading" class="p-datatable-sm mt-3">
      <Column field="systemNumber" header="№" />
      <Column field="title" header="Название" />
      <Column field="process.name" header="Тип" />
      <Column field="createdAtUtc" header="Создан" />

      <Column header="Открыть">
        <template #body="{ data }">
          <Button icon="pi pi-angle-right" @click="open(data.id)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import documentApi from '@/api/documentApi'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'

const route = useRoute()
const router = useRouter()

const documents = ref([])
const loading = ref(false)
const pageTitle = ref("")

const loadDocuments = async () => {
  loading.value = true

  const processId = route.query.processId
  const type = route.path.replace("/", "")   // my / todo / archive

  if (!processId) {
    documents.value = []
    return
  }

  let response

  if (type === "my") {
    pageTitle.value = "Мои документы"
    response = await documentApi.getMyDocuments()
  } else if (type === "todo") {
    pageTitle.value = "Документы на согласовании"
    response = await documentApi.getTodoDocuments()
  } else if (type === "archive") {
    pageTitle.value = "Архив документов"
    response = await documentApi.getArchiveDocuments()
  }

  // фильтруем только нужный процесс
  documents.value = response.data.filter(doc => doc.processId === processId)

  loading.value = false
}

onMounted(loadDocuments)

// обновляем список при смене processId
watch(() => route.query.processId, loadDocuments)

const open = (id) => {
  router.push(`/documents/view/${id}`)
}
</script>

<style scoped>
.page {
  padding: 20px;
}
</style>