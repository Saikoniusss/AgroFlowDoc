<template>
  <Card class="p-1 border-2">
    <template #header>
      <Card class="border-1">
        <template #content>
          <div class="grid">
            <div class="w-10 grid">
              <h3>№ {{ document?.systemNumber }}</h3>
            </div>
            <div class="w-2 flex justify-content-end align-items-center gap-2">
            <Tag :value="mappedStatus" :severity="statusColor" />
              <span class="font-semibold">
                {{ document?.displayName }}
              </span>

              <span class="text-sm text-color-secondary">
                {{ formatDateTime(document?.createdAtUtc) }}
              </span>
          </div>
          </div>
        </template>
      </Card>
    </template>
    <template #content>
        <div class="flex flex-column gap-4 w-full">
      <!-- 📝 Данные документа -->
          <Card class="w-full border-0">
            <template #title>📝 Данные документа</template>
            <template #content>
              <div
                v-for="(value, key) in fields"
                :key="key"
                class="grid m-1 border-bottom-1 py-2"
                style="text-align: left; font-weight: 300;"
              >
                <div class="w-4">
                  <strong>{{ getLabel(key) }}:</strong>
                </div>

                <div class="w-8" v-if="isDateValue(value) ">
                  <span>{{ formatDateTime(value) }}</span>
                </div>
                <div class="w-8" v-else>
                  <span>{{ value }}</span>
                </div>
              </div>
            </template>
          </Card>
          <Card class="w-full border-0">
            <template #title>📌 Маршрут согласования</template>
            <template #content>
              <DataTable :value="steps" class="p-datatable-sm w-full">

                <Column field="stepOrder" header="№" style="width: 60px" />

                <Column field="stepName" header="Этап" />

                <Column header="Исполнители">
                  <template #body="{ data }">
                    <ul v-if="data.executors?.length">
                      <li v-for="ap in data.executors" :key="ap.id">
                        {{ ap.displayName }}
                      </li>
                  </ul> 
                  <span v-else>—</span>
                  </template>
                </Column>

                <Column header="Дата подписания" style="width: 180px">
                  <template #body="{ data }">
                    <span v-if="data.completedAtUtc">
                      {{ formatDateTime(data.completedAtUtc) }}
                    </span>
                    <span v-else>—</span>
                  </template>
                </Column>

                <Column header="Статус" style="width:120px">
                  <template #body="{ data }">
                    <StatusIcon :status="data.status" />
                  </template>
                </Column>

              </DataTable>
            </template>
          </Card>
          <Card class="w-12 border-0">
            <template #title>📎 Вложения</template>
            <template #content>
              <div v-if="document?.files && document?.files.length > 0">
                <DataTable :value="document?.files" class="p-datatable-sm w-full">
                  <Column field="fileName" header="Имя файла" />
                  <Column header="Действия">
                    <template #body="{ data }">
                      <a :href="'/' + data.relativePath" target="_blank"><i class="pi pi-eye m-1 cursor-pointer"></i></a>
                      <a :href="'/' + data.relativePath" target="_blank"><i class="pi pi-download m-1 cursor-pointer"></i></a>
                    </template>
                  </Column>
                </DataTable>
              </div>
              <div v-else>
                <span class="text-yellow-900">Файлы не прикреплнены</span>
              </div>
            </template>
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

import Card from 'primevue/card'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import http from '../../api/http'
import { DateTime } from 'luxon';
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import StatusIcon from '@/components/StatusIcon.vue'

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
  Черновик: "Черновик",
  Согласование: "Согласование",
  Утверждён: "Утверждён",
  Отклонён: "Отклонён"
}

const mappedStatus = computed(() => statusMap[document.value?.status] ?? document.value?.status)

const statusColor = computed(() => {
  switch (mappedStatus.value) {
    case "Утверждён": return "success"
    case "Отклонён": return "danger"
    case "Согласование": return "warning"
    default: return "secondary"
  }
})
function formatApprover(value) {
  console.log(value)
  if (value.startsWith("role:"))
    return "Роль: " + value.replace("role:", "");

  if (value.startsWith("user:"))
    return "Пользователь: " + value.replace("user:", "");

  return value;
}
function isDateValue(value) {
  if (typeof value === "number") return false; // число → НЕ дата
  if (typeof value !== "string") return false;

  // ISO-подобная строка
  return /^\d{4}-\d{2}-\d{2}/.test(value);
}
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
  http.get(`/v1/documents/files/${f.id}/download`);

const approve = async () => {
  await http.post(`/v1/documents/${route.params.id}/approve`)
  router.push('/todo')
}

const reject = async () => {
  const comment = prompt("Причина отклонения:")
  if (!comment) return
  await http.post(`/${route.params.id}/reject`, { comment })
  router.push('/todo')
}
</script>

<style scoped>
.p-card-body {
  margin: 2px !important;
}
</style>