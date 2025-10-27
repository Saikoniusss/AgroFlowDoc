<script setup>
import { Menubar, PanelMenu } from 'primevue';
import router from '../../router'
</script>

<template>
    <div class="layout-container flex flex-column h-screen">
        <Menubar :model="[
                {
                    icon: 'pi pi-search'
                },
                {
                    icon: 'pi pi-info-circle'
                },
                {
                    icon: 'pi pi-sync'
                },
                {
                    icon: 'pi pi-star'
                },
                {
                    icon: 'pi pi-wrench'
                },
                {
                    icon: 'pi pi-clock'
                },
                {
                    icon: 'pi pi-bars'
                },
                {
                    icon: 'pi pi-table'
                }
        ]">
            <template #start>
                <span class="layout-logo text-xl font-bold ml-2">AgroFlow</span>
            </template>
        </Menubar>
        <div class="layout-content flex flex-1">
            <aside class="layout-sidebar surface-50 border-right-1 border-gray-200 flex flex-column":class="{ hidden: !sidebarVisible && isMobile }">
                <PanelMenu :model="[
                    {
                        label: 'Заявка',
                        icon: 'pi pi-fw pi-home',
                        command: () => router.push('/application')
                    },
                    {
                        label: 'Счет на оплату',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/invoice')
                    },
                    {
                        label: 'Заявка на оприходование на склад',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/receipt-request')
                    },
                    {
                        label: 'Завка на отпуск со склада',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/issue-request')
                    },
                    {
                        label: 'Счет на оплату расходы на экспорт',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/export-expense-invoice')
                    },
                    {
                        label: 'Картотека отгрузок на экспорт',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/export-shipments-archive')
                    },
                    {
                        label: 'Склад',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/warehouse')
                    },
                    {
                        label: 'Прием зерна',
                        icon: 'pi pi-fw pi-info-circle',
                        command: () => router.push('/grain-reception')
                    }
                ]" class="flex-1 overflow-auto" />
            </aside>
            <main class="layout-main flex-1 overflow-auto p-4">
                <router-view />
            </main>
        </div>
    </div>
</template>

<style scoped>
.layout-container {
    height: 100vh;
    overflow: hidden;
}

.layout-content {
    flex: 1;
    overflow: hidden;
}

.layout-sidebar {
    width: 350px;
    transition: transform 0.3s ease;
}

.layout-main {
    background-color: var(--surface-card);
}

/* Мобильная версия — скрываем сайдбар */
@media (max-width: 767px) {
    .layout-sidebar.hidden {
        display: none;
    }
}
</style>