<template>
  <Card class="p-1 border-2">
    <template #header>
      <Card class="border-1">
        <template #content>
          <div class="grid">
            <div class="w-10 grid">
              <h3>№ {{ document?.systemNumber }}</h3>
            </div>
            <div class="w-2">
              <Tag :value="statusMap[document?.status]" :severity="statusColor" />
              <b>{{ formatDateTime(document?.createdAtUtc) }}</b>
            </div>
          </div>
        </template>
      </Card>
    </template>
    <template #content>
        <div class="grid">
          <Card class="w-4 border-0">
            <template #title>📝 Данные документа</template>
            <template #content>
                <div v-for="(value, key) in fields" :key="key" class="grid m-1 border-bottom-1" style="text-align: left; font-weight: 300;">
                  <div class="w-4">
                    <strong>{{ getLabel(key) }}:</strong>
                  </div>
                  <div class="w-8" v-if="isDate(value)">
                    <span>{{ formatDateTime(value) }}</span>
                  </div>
                  <div class="w-8" v-else>
                    <span>{{ value }}</span>
                  </div>
                </div>
            </template>
          </Card>
          <Card class="w-8 border-0">
            <template #title>📌 Маршрут согласования</template>
            <template #content>
              <Timeline :value="steps" align="alternate" class="customized-timeline">
                  <template #marker="slotProps">
                      <span>
                          {{ slotProps.item.stepOrder }}
                      </span>
                  </template>
                  <template #content="slotProps">
                      <Card class="mt-1 border-1">
                          <template #title>
                              {{ slotProps.item.status }}
                          </template>
                          <template #subtitle>
                              {{ slotProps.item.stepName }}
                          </template>
                      </Card>
                  </template>
              </Timeline>
            </template>
          </Card>
          <Card class="w-4 border-0">
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
    </template>
    <template #footer>
      <div class="flex gap-4 mt-1">
        <Button
          v-if="document?.canApprove"
          label="Одобрить"
          class="p-button-success w-full"
          icon="pi pi-check"
          @click="approve"
        />

        <Button
          v-if="document?.canApprove"
          label="Отклонить"
          class="p-button-danger w-full"
          icon="pi pi-times"
          @click="reject"
        />
      </div>
    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import documentApi from '@/api/documentApi'

import Card from 'primevue/card'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import Timeline from 'primevue/timeline'
import http from '../../api/http'
import { DateTime } from 'luxon';

const route = useRoute()
const router = useRouter()
const document = ref(null)

const fields = ref({})
const steps = ref([])

const formatDateTime = (utcString) => {
  return DateTime.fromISO(utcString, { zone: 'utc' }) // берём UTC
    .setZone('Asia/Yekaterinburg') // конвертируем в Екатеринбург
    .toFormat('dd.MM.yyyy, HH:mm'); // формат 24 часа
};

const isDate = (str) => {
  const d = new Date(str);
  return !isNaN(d.getTime());
}

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
  const { data } = await http.get(`/v1/documents/${route.params.id}`)

  document.value = data
  fields.value = JSON.parse(data.fieldsJson || "{}")
  steps.value = data.workflow
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
.p-card-body {
  margin: 2px !important;
}
</style>