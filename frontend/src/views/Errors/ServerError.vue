<template>
    <div class="error-page">
        <h1>500 — Внутренняя ошибка сервера</h1>
        <p>Произошла ошибка на сервере. Пожалуйста, попробуйте позже.</p>
        <router-link to="/documents">На главную</router-link>
    </div>
</template>

<style scoped>
.error-page {
    text-align: center;
    margin-top: 100px;
}
</style>