<template>
  <div class="login">
    <h2>Вход через Telegram</h2>
    <div id="telegram-login"></div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'

onMounted(() => {
  // создаём элемент <script>, чтобы подключить Telegram Widget
  const script = document.createElement('script')
  script.async = true
  script.src = 'https://telegram.org/js/telegram-widget.js?22'

  // ⚙️ Указываем имя бота и URL твоего backend (через Cloudflared)
  script.setAttribute('data-telegram-login', 'AgroRequestSenderBot') // без @
  script.setAttribute('data-size', 'large')
  script.setAttribute('data-userpic', 'false')
  script.setAttribute('data-request-access', 'write')

  // ⚡️ Вот эта строка — ключевая:
  script.setAttribute('data-auth-url', 'https://stirring-sole-16.rshare.io/api/v1/auth/telegram')

  document.getElementById('telegram-login').appendChild(script)
})
</script>

<style scoped>
.login {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-top: 100px;
}
</style>