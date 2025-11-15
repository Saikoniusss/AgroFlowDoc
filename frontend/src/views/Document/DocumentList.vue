<template>
  <div class="page">
    <h2>📄 Создать новый документ</h2>

    <DataTable :value="processes" :loading="loading">
      <Column field="name" header="Название процесса" />
      <Column field="templateName" header="Шаблон" />
      <Column>
        <template #body="{ data }">
          <Button label="Создать" icon="pi pi-plus"
                  @click="openCreate(data.id)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import { useRouter, useRoute } from "vue-router"
import http from '../../api/http'

const route = useRoute()
const router = useRouter()
const processes = ref([])
const loading = ref(false)

const load = async () => {
  loading.value = true
  try {
    const { data } = http.get('/v1/documents/processes')
    processes.value = data
  } finally {
    loading.value = false
  }
}

const openCreate = (id) => {
  router.push(`/documents/create/${id}`)
}

onMounted(load)
</script>

<style scoped>
.page { padding: 20px; }
</style>