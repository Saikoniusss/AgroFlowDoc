<template>
  <Card class="p-1 border-2">
    <template #header>
      <h2>📄 Создать новый документ</h2>
    </template>
    <template #content>
      <DataView :value="processes" :loading="loading" dataKey="id" paginator :rows="4">
        <template #list="slotProps">
          <div class="flex flex-row gap-3">
            <div class="doc-card w-4" v-for="item in slotProps.items" :key="item.id">
                <div class="doc-header">
                    <span class="doc-id">Код:</span>
                    <span class="status status-info">{{ item.code }}</span>
                </div>

                <div class="doc-name">{{ item.templateName }}</div>

                <div class="doc-info">
                    <div><strong>Процесс:</strong> {{ item.name }}</div> 
                </div>

                <div class="doc-actions">
                  <Button label="Создать" icon="pi pi-plus"
                    @click="openCreate(item.id)" />
                </div>
            </div>
          </div>
        </template>
      </DataView>

    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import DataView from "primevue/dataview"
import Button from 'primevue/button'
import { useRouter, useRoute } from "vue-router"
import http from '../../api/http'
import { Card } from 'primevue'

const router = useRouter()
const processes = ref([])
const loading = ref(false)

const load = async () => {
  loading.value = true
  try {
    const { data } = await http.get('/v1/documents/processes')
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
</style>