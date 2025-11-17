<template>
  <div class="page">
    <h2>{{ pageTitle }}</h2>
      <DataView :value="documents" layout="list" dataKey="id" paginator :rows="4">
        <template #list="slotProps">
            <div class="flex flex-column gap-3" style="background-color: #d7cfcf;">
                <div v-for="item in slotProps.items" :key="item.id" class="p-3 border-1 surface-border surface-0 border-round grid text-left mr-0 ml-0">
                    <div class="w-1">
                        <img src="https://primefaces.org/cdn/primevue/images/avatar/amyelsner.png" alt="Document Image" class="w-6 border-round">
                    </div>
                    <div class="w-8 grid">
                        <div class="text-xl font-bold w-8">{{ item.title }}</div>
                        <div class="font-bold w-4">№ {{ item.systemNumber }} от {{ item.createdAtUtc }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Отправитель</div>
                        <div class="w-4">Иванов И.И.</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Статус</div>
                        <div class="w-4">{{ item.status }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-8">Тип</div>
                        <div class="w-4">{{ item.process.name }}</div>
                        <Divider class="my-1" />
                        <div class="text-md w-12">Контрольный срок: Нет</div>
                    </div>
                    <div class="w-3 text-right">
                      <Button icon="pi pi-angle-right" @click="open(item.id)" />
                    </div>
                </div>
            </div>
        </template>
    </DataView>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import http from '../../api/http'
import DataView from "primevue/dataview"
import Divider from 'primevue/divider';

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

const open = (id) => {
  router.push(`/documents/view/${id}`)
}
</script>

<style scoped>
</style>