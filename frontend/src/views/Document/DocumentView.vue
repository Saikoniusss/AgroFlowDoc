<template>
  <div class="page">

    <!-- Заголовок документа -->
    <div class="header-card">
      <h2>{{ document?.title }}</h2>
      <span class="system-number">№ {{ document?.systemNumber }}</span>
      <Tag :value="statusMap[document?.status]" :severity="statusColor" />
    </div>

    <div class="grid mt-4">

      <!-- Левая колонка -->
      <div class="col-8">
        
        <!-- Блок полей -->
        <Card class="mb-3">
          <template #title>📝 Данные документа</template>

          <div class="field-list">
            <div v-for="(value, key) in fields" :key="key" class="field-row">
              <strong>{{ getLabel(key) }}:</strong>
              <span>{{ value }}</span>
            </div>
          </div>
        </Card>

        <!-- Файлы -->
        <Card class="mb-3">
          <template #title>📎 Вложения</template>

          <ul v-if="document.files.length > 0">
            <li v-for="file in document.files" :key="file.id">
              <a :href="fileUrl(file)" target="_blank">
                <i class="pi pi-file" style="margin-right: 6px"></i>
                {{ file.fileName }}
              </a>
            </li>
          </ul>

          <div v-else class="text-muted">
            Файлы не прикреплены
          </div>
        </Card>

      </div>

      <!-- Правая колонка -->
      <div class="col-4">

        <!-- Маршрут согласования -->
        <Card>
          <template #title>📌 Маршрут согласования</template>

          <Timeline :value="steps">
            <template #content="{ item }">
              <div>
                <b>{{ item.stepOrder }}. {{ item.stepName }}</b><br />
                <span :class="`status ${item.status.toLowerCase()}`">
                  {{ item.status }}
                </span>
              </div>
            </template>
          </Timeline>
        </Card>

      </div>

    </div>

    <!-- Кнопки -->
    <div class="actions mt-4">

      <Button
        v-if="document?.canApprove"
        label="Одобрить"
        class="p-button-success"
        icon="pi pi-check"
        @click="approve"
      />

      <Button
        v-if="document?.canApprove"
        label="Отклонить"
        class="p-button-danger ml-2"
        icon="pi pi-times"
        @click="reject"
      />
      
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import documentApi from '@/api/documentApi'

import Card from 'primevue/card'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import Timeline from 'primevue/timeline'

const route = useRoute()
const router = useRouter()
const document = ref(null)

const fields = ref({})
const steps = ref([])

const statusMap = {
  Draft: "Черновик",
  InProgress: "На согласовании",
  Approved: "Утвержден",
  Rejected: "Отклонён"
}

const statusColor = computed(() => {
  switch (document.value?.status) {
    case "Approved": return "success"
    case "Rejected": return "danger"
    case "InProgress": return "warning"
    default: return "secondary"
  }
})

onMounted(async () => {
  const { data } = await documentApi.getDocument(route.params.id)

  document.value = data
  console.log(document.value)
  fields.value = JSON.parse(data.fieldsJson || "{}")
    console.log(fields.value)
  steps.value = data.workflowTrackers
})

const getLabel = (key) => {
  const field = document.value.template.fields.find(f => f.name === key)
  return field?.label || key
}

const fileUrl = (f) =>
  `${import.meta.env.VITE_API_BASE_URL}/${f.relativePath}`

const approve = async () => {
  await documentApi.approve(route.params.id)
  router.push('/todo')
}

const reject = async () => {
  const comment = prompt("Причина отклонения:")
  if (!comment) return
  await documentApi.reject(route.params.id, comment)
  router.push('/todo')
}
</script>

<style scoped>
.header-card {
  display: flex;
  align-items: center;
  gap: 20px;
}
.system-number {
  color: #666;
  font-size: 14px;
}
.field-row {
  display: flex;
  justify-content: space-between;
  padding: 6px 0;
  border-bottom: 1px solid #eee;
}
.status.pending { color: orange; }
.status.approved { color: green; }
.status.rejected { color: red; }
</style>