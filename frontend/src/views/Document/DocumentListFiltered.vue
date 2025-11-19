<template>
  <Card class="p-1 border-2">
    <template #header>
      <h2>{{ pageTitle }}</h2>
    </template>
    <template #content>
      <DataView :value="documents" layout="list" dataKey="id" paginator :rows="4">
        <template #list="slotProps">
            <div class="flex flex-column gap-3" style="background-color: #d7cfcf;">
                <div v-for="item in slotProps.items" :key="item.id" class="p-3 surface-0 border-round grid text-left mr-0 ml-0" style="border: 1px solid black;">
                    <div class="w-1">
                        <img src="https://primefaces.org/cdn/primevue/images/avatar/amyelsner.png" alt="Document Image" class="w-6 border-round">
                    </div>
                    <div class="w-8 grid">
                        <div class="text-xl font-bold w-8">{{ item.process.name }}</div>
                        <div class="font-bold w-4">№ {{ item.systemNumber }} от {{ formatDateTime(item.createdAtUtc) }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Автор</div>
                        <div class="w-4">{{ item.createdByDisplayName}}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Описание</div>
                        <div class="w-4">{{ item.title }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Статус</div>
                        <div class="w-4">{{ item.status }}</div>
                        <Divider class="my-1" />
                    </div>
                    <div class="w-3 text-right">
                      <Button icon="pi pi-angle-right" @click="open(item.id)" />
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
console.log(documents)
  loading.value = false
}

onMounted(loadDocuments)

// обновляем список при смене processId
watch(() => route.query.processId, loadDocuments)
watch(
  () => route.fullPath,
  () => loadDocuments()
)

const open = (id) => {
  router.push(`/documents/view/${id}`)
}
</script>

<style scoped>
</style>