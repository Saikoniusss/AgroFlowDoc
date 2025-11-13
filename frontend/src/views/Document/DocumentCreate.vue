<template>
  <div class="page">
    <h2>Создание документа</h2>

    <div v-if="process">
      <h3>{{ process.documentTemplate }}</h3>

      <div class="form">
        <label>Заголовок</label>
        <input v-model="title" type="text" />

        <div v-for="f in process.documentTemplate?.fields" :key="f.id" class="field">
          <label>{{ f.label }}</label>

          <input v-if="f.fieldType === 'text'" v-model="fields[f.name]" type="text" />

          <input v-if="f.fieldType === 'number'" v-model="fields[f.name]" type="number" />

          <input v-if="f.fieldType === 'date'" v-model="fields[f.name]" type="date" />

          <select v-if="f.fieldType === 'select'" v-model="fields[f.name]">
            <option v-for="o in JSON.parse(f.optionsJson)" :key="o" :value="o">
              {{ o }}
            </option>
          </select>
        </div>

        <Button label="Создать" icon="pi pi-check" @click="create" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import documentApi from "@/api/documentApi"
import { useRouter, useRoute } from "vue-router"

const route = useRoute()
const router = useRouter()

const process = ref(null)
const fields = ref({})
const title = ref("")

onMounted(async () => {
  const { data } = await documentApi.getProcess(route.params.processId)
  process.value = data

  // Заполняем дефолтные поля
  data.documentTemplate.fields.forEach(f => {
    fields.value[f.name] = ""
  })
})

const create = async () => {
  const payload = {
    processId: process.value.id,
    title: title.value,
    fieldsJson: JSON.stringify(fields.value)
  }

  const { data } = await documentApi.createDocument(payload)
  router.push(`/documents/${data.id}`)
}
</script>

<style scoped>
.field {
  margin-bottom: 14px;
}
.form {
  margin-top: 20px;
  max-width: 400px;
}
</style>