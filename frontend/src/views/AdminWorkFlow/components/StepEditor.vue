<template>
  <div class="p-fluid">
    <div class="p-field">
      <label>Название этапа</label>
      <InputText v-model="stepLocal.stepName" />
    </div>

    <div class="p-formgrid grid">
      <div class="field col">
        <label>Порядок</label>
        <InputNumber v-model="stepLocal.stepOrder" :min="1" />
      </div>
      <div class="field col">
        <label>Мин. утверждений</label>
        <InputNumber v-model="stepLocal.minApprovals" :min="1" />
      </div>
    </div>

    <div class="p-field-checkbox">
      <Checkbox v-model="stepLocal.isParallel" binary />
      <label class="ml-2">Параллельное согласование</label>
    </div>

    <Divider />
    <h4>Согласующие:</h4>

    <TabView>
      <TabPanel header="Роли">
        <div v-for="r in roles" :key="r.id" class="p-field-checkbox">
          <Checkbox
            :inputId="'role-' + r.id"
            :value="'role:' + r.name"
            v-model="selectedApprovers"
          />
          <label :for="'role-' + r.id">{{ r.name }}</label>
        </div>
      </TabPanel>

      <TabPanel header="Пользователи">
        <div v-for="u in users" :key="u.id" class="p-field-checkbox">
          <Checkbox
            :inputId="'user-' + u.id"
            :value="'user:' + u.id"
            v-model="selectedApprovers"
          />
          <label :for="'user-' + u.id">{{ u.displayName }}</label>
        </div>
      </TabPanel>
    </TabView>

    <Divider />
    <div class="flex justify-content-end gap-2 mt-3">
      <Button label="Отмена" text @click="$emit('cancel')" />
      <Button label="Сохранить" icon="pi pi-check" @click="saveStep" />
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Checkbox from 'primevue/checkbox'
import Divider from 'primevue/divider'
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import adminWorkflowApi from '@/api/adminWorkflowApi'

const props = defineProps({
  step: Object,
  mode: String,
  route: Object
})
const emit = defineEmits(['save', 'cancel'])

const stepLocal = ref({})
const users = ref([])
const roles = ref([])
const selectedApprovers = ref([])

watch(
  () => props.step,
  (val) => {
    stepLocal.value = JSON.parse(JSON.stringify(val || {}))
    try {
      selectedApprovers.value = JSON.parse(val?.approversJson || '[]')
    } catch {
      selectedApprovers.value = []
    }
  },
  { immediate: true }
)

const loadUsersAndRoles = async () => {
  const [u, r] = await Promise.all([
    adminWorkflowApi.getUsers(),
    adminWorkflowApi.getRoles()
  ])
  users.value = u.data
  roles.value = r.data
}

const saveStep = () => {
  const payload = {
    ...stepLocal.value,
    approversJson: JSON.stringify(selectedApprovers.value)
  }
  emit('save', payload)
}

onMounted(loadUsersAndRoles)
</script>

<style scoped>
.p-field-checkbox {
  margin-bottom: 0.3rem;
}
</style>