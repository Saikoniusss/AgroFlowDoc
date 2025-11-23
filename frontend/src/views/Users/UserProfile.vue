<script setup>
import { ref, onMounted, computed } from 'vue';
import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Avatar from 'primevue/avatar';
import http from '@/api/http';
import { useAuthStore } from '@/store/auth';

const profile = ref({});
const message = ref('');

onMounted(async () => {
  const response =  await http.get('/Profile/me');
  profile.value = response.data;
});

const avatarPath = computed(() => {
    if (profile.value.photo) {
      return http.defaults.baseURL.replace('/api', '') + '/' + profile.value.photo;
    } else if (profile.value.avatarPath) {
      return http.defaults.baseURL.replace('/api', '') + '/' + profile.value.avatarPath;
    }
    return null;
  }
)

const updateProfile = async () => {
  await http.put('/Profile/update', profile.value).then(response => {
    message.value = 'Профиль успешно обновлен.';
  }).catch(error => {
    message.value = 'Ошибка при обновлении профиля.';
  });
};

const onFileChange = async (event) => {
  const file = event.target.files[0];
  if (file) {
    const formData = new FormData();
    formData.append('photo', file);

    try {
      const response = await http.post('/Profile/upload-avatar', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      profile.value.photo = response.data.photo;
      useAuthStore().fetchUser();
      message.value = 'Аватар успешно обновлен.';
    } catch (error) {
      message.value = 'Ошибка при загрузке аватара.';
    }
  }
};

</script>

<template>
  <Card style="width: 50%; overflow: hidden" class="m-auto">
    <template #title>
      Профиль пользователя
    </template>
    <template #subtitle>
      Управление информацией аккаунта
    </template>
    <template #content>
      <form>
        
        <div class="flex justify-center mb-4">
          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            class="hidden"
            @change="onFileChange"
          />
          <Avatar
            :image="avatarPath"
            :label="avatarPath ?  null : profile.displayName ? profile.displayName.charAt(0).toUpperCase() : ''"
            shape="circle"
            class="m-auto"
            style="width: 150px; height: 150px;"
            @click="$refs.fileInput.click()"
          />
        </div>
        <div class="flex flex-col gap-1 mb-4">
            <InputText v-model="profile.displayName" type="text" placeholder="Имя" fluid />
        </div>
        <div class="flex flex-col gap-1 mb-4">
            <InputText v-model="profile.email" type="text" placeholder="Email" fluid />
        </div>
        <div class="flex flex-col gap-1 mb-4">
            <InputText v-model="profile.telegramUsername" type="text" placeholder="@username" fluid />
        </div>
      </form>
    </template>
    <template #footer>
      <div class="tg-status">
        <template v-if="profile.telegramChatId">
          ✅ Telegram привязан (Chat ID: {{ profile.telegramChatId }})
        </template>
        <template v-else>
          🔗 Напиши сообщение боту
          <a href="https://t.me/AgroRequestSenderBot" target="_blank">@AgroRequestSenderBot</a>,
          после чего система автоматически свяжет аккаунт.
        </template>
      </div>
      <Button @click="updateProfile" severity="secondary" label="Сохранить" fluid />
      <div class="mt-2 text-sm text-gray-500" v-if="message">
        {{ message }}
      </div>
    </template>
  </Card>
</template>

<style scoped>

</style>