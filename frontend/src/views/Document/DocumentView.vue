<template>
  <div class="page" v-if="document">
    <h2>{{ document.title }}</h2>

    <div class="card p-3 mt-3">

      <h3>Поля документа</h3>
      <div class="fields">
        <div v-for="(val, key) in document.fieldsJson" :key="key" class="field-item">
          <strong>{{ key }}</strong>: {{ val }}
        </div>
      </div>

      <h3 class="mt-4">Маршрут согласования</h3>
      <ul>
        <li v-for="step in document.workflowTrackers" :key="step.id">
          {{ step.stepOrder }}. {{ step.stepName }} — 
          <strong :class="step.status.toLowerCase()">{{ step.status }}</strong>
        </li>
      </ul>

      <h3 class="mt-4">Файлы</h3>
      <ul>
        <li v-for="f in document.files" :key="f.id">
          <a :href="`http://localhost:5097/${f.relativePath}`" target="_blank">{{ f.fileName }}</a>
        </li>
      </ul>

      <div class="actions mt-4">
        <Button label="Одобрить" class="p-button-success" @click="approve" />
        <Button label="Отклонить" class="p-button-danger ml-2" @click="reject" />
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import documentApi from '@/api/documentApi'
import http from '@/api/http';

const route = useRoute()
const router = useRouter()
const document = ref(null)

onMounted(async () => {
  const { data } = await http.get(`/v1/documents/${route.params.id}`)
  data.fieldsJson = JSON.parse(data.fieldsJson || "{}")
  document.value = data
})

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
.field-item {
  margin-bottom: 5px;
}

.pending {
  color: orange;
}
.approved {
  color: green;
}
.rejected {
  color: red;
}
</style>