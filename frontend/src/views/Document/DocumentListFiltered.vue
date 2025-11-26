<template>
  <Card class="p-1 border-2">
    <template #header>
      <h2>{{ pageTitle }}</h2>
    </template>
    <template #content>
      <DataView :value="documents" layout="list" dataKey="id" paginator :rows="4">
        <template #list="slotProps">
            <div class="flex flex-column gap-3">
                <div class="cards-vertical text-left">
                  <div v-for="item in slotProps.items" :key="item.id" class="doc-card mb-2  ">
                    <div class="doc-header">
                        <span class="doc-id">Код: {{ item.systemNumber }}</span>
                        <span :class="'status status-' + statusColor(item.status)">{{ item.status }}</span>
                    </div>

                    <div class="doc-name">{{ item.title }}</div>

                    <div class="doc-info">
                        <div><strong>Тип:</strong> {{ item.process.name }}</div>
                        <div><strong>Дата:</strong> {{ formatDateTime(item.createdAtUtc) }}</div>
                        <div><strong>Автор:</strong> {{ item.createdByDisplayName }}</div>
                    </div>

                    <div class="doc-actions">
                        <Button @click="open(item.id)"> Просмотр  </Button>
                    </div>
                </div>
                </div>
            </div>
        </template>
      </DataView>
    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import http from '../../api/http'
import DataView from "primevue/dataview"
import { DateTime } from 'luxon';
import { Card } from 'primevue'

const route = useRoute()
const router = useRouter()

const documents = ref([])
const loading = ref(false)
const pageTitle = ref("")

const formatDateTime = (utcString) => {
  return DateTime.fromISO(utcString, { zone: 'utc' }) // берём UTC
    .setZone('Asia/Yekaterinburg') // конвертируем в Екатеринбург
    .toFormat('dd.MM.yyyy, HH:mm'); // формат 24 часа
};

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
    response = await http.get('/v1/documents/my')
  } else if (type === "todo") {
    pageTitle.value = "Документы на согласовании"
    response = await http.get('/v1/documents/todo')
  } else if (type === "archive") {
    pageTitle.value = "Архив документов"
    response = await http.get('/v1/documents/archive')
  }

  // фильтруем только нужный процесс
  documents.value = response.data.filter(doc => doc.process.id === processId)
  loading.value = false
}

onMounted(loadDocuments)

// обновляем список при смене processId
watch(() => route.query.processId, loadDocuments)
watch(
  () => route.fullPath,
  () => loadDocuments()
)

const statusColor = function(status) {
  switch (status) {
    case "Утверждён": return "success"
    case "Отклонён": return "danger"
    case "Согласование": return "warning"
    default: return "secondary"
  }
}

const open = (id) => {
  router.push(`/documents/view/${id}`)
}
</script>

<style scoped>
</style>