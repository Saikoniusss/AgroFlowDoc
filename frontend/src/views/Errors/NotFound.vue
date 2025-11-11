<template>
    <div class="error-page">
        <h1>404 — Страница не найдена</h1>
        <p>Такой страницы не существует.</p>
        <router-link to="/documents">На главную</router-link>
    </div>
</template>

<style scoped>
.error-page {
    text-align: center;
    margin-top: 100px;
}
</style>