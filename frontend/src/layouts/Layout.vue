<script setup>
import { Menubar, PanelMenu, Button, Menu } from 'primevue';
import router from '../router';
import routerPage from '../routers/index';
import { useAuthStore } from '@/store/auth';
import { ref, computed } from 'vue';

const auth = useAuthStore();
const sidebarVisible = ref(true);
const isMobile = computed(() => window.innerWidth <= 767);

// 👇 управление меню профиля
const menu = ref();
const menuItems = ref([
  {
    label: 'Редактировать профиль',
    icon: 'pi pi-user-edit',
    command: () => routerPage.push('/profile'),
  },
  {
    separator: true,
  },
  {
    label: 'Выйти',
    icon: 'pi pi-sign-out',
    command: async () => {
      await auth.logout();
      routerPage.push('/login');
    },
  },
]);
const toggleMenu = (event) => {
  if (menu.value && typeof menu.value.toggle === 'function') {
    menu.value.toggle(event);
  }
};

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
                <span class="layout-logo text-xl font-bold ml-2 mr-2">AgroFlow!</span>
            </template>
            <template #end>
            <div class="flex align-items-center gap-2">
                <Button
                    icon="pi pi-user"
                    :label="auth.user?.displayName || auth.user?.username || 'Профиль'"
                    class="p-button-text p-button-plain"
                    @click="toggleMenu"
                />
                <Menu ref="menu" :model="menuItems" :popup="true" />
            </div>
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