<template>
  <div>
    <i v-if="status === 'Pending'" class="pi pi-spinner pi-spin" />
    <i v-else-if="status === 'Approved'" class="pi pi-check text-green-500" />
    <i v-else-if="status === 'Rejected'" class="pi pi-times text-red-500" />
    <i v-else-if="status === 'Waiting'" class="pi pi-clock" />
    <span v-else>{{ status }}</span>
  </div>
</template>

<script setup>
defineProps({
  status: String
});
</script>