<template>
  <div class="page">
    <h2>📄 Создание документа</h2>

    <div class="grid">
      <div
        v-for="p in processes"
        :key="p.id"
        class="card"
        @click="openProcess(p.id)"
      >
        <h3>{{ p.documentTemplate.name }}</h3>
        <p>{{ p.documentTemplate.name }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import documentApi from "@/api/documentApi"
import { useRouter } from "vue-router"

const processes = ref([])
const router = useRouter()

onMounted(async () => {
  const { data } = await documentApi.getProcesses()
  processes.value = data
})

const openProcess = (id) => {
  router.push(`/documents/create/${id}`)
}
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, 260px);
  gap: 16px;
}
.card {
  padding: 16px;
  background: white;
  border-radius: 12px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}
.card:hover {
  background: #f3f3f3;
}
</style>