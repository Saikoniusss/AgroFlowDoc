<script setup>
import { Menubar, PanelMenu, Button, Menu, Avatar } from 'primevue';
import routerPage from '../routers/index';
import { useAuthStore } from '@/store/auth';
import { ref, computed, onMounted  } from 'vue';
import { useRouter } from 'vue-router';
import http from '../api/http';

const router = useRouter();
const auth = useAuthStore();
console.log(auth)
const sidebarVisible = ref(true);
const isMobile = computed(() => window.innerWidth <= 767);
// 👇 управление меню профиля
const menuProfile  = ref();
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
        },
    },
]);
const toggleMenu = (event) => {
    if (menuProfile.value && typeof menuProfile.value.toggle === 'function') {
        menuProfile.value.toggle(event);
    }
};
// 🔥 ДИНАМИЧЕСКОЕ МЕНЮ ДОКУМЕНТОВ
const slideMenu = ref([]);
onMounted(async () => {
    try {
        const { data } = await http.get('/v1/documents/menu-counts')

        slideMenu.value = [
            {
                label: 'Мои документы',
                icon: 'pi pi-list-check',
                items: data.map(p => ({
                    label: `${p.processName} (${p.my})`,
                    command: () => router.push(`/my?processId=${p.processId}`)
                }))
            },
            {
                label: 'На согласовании',
                icon: 'pi pi-eye',
                items: data.map(p => ({
                    label: `${p.processName} (${p.todo})`,
                    command: () => router.push(`/todo?processId=${p.processId}`)
                }))
            },
            {
                label: 'Архив',
                icon: 'pi pi-box',
                items: data.map(p => ({
                    label: `${p.processName} (${p.archive})`,
                    command: () => router.push(`/archive?processId=${p.processId}`)
                }))
            }
        ];
    }
    catch (err) {
        console.error('Ошибка загрузки меню', err);
    }
});
</script>


<template>
    <div class="layout-container flex flex-column h-screen">
        <Menubar :model="[
                {
                    icon: 'pi pi-cog',
                    visible: auth.user?.roles?.includes('Administrator'),
                    items: [
                        {
                            label: 'Пользователи',
                            icon: 'pi pi-users',
                            command: () => router.push('/admin/users'),
                            visible: auth.user?.roles?.includes('Administrator'),
                        },
                        {
                            label: 'Роли',
                            icon: 'pi pi-key',
                            command: () => router.push('/admin/roles'),
                            visible: auth.user?.roles?.includes('Administrator'),
                        },
                        {
                            label: 'Маршруты',
                            icon: 'pi pi-sitemap',
                            command: () => router.push('/admin/routes'),
                            visible: auth.user?.roles?.includes('Administrator'),
                        },
                        {
                            label: 'Шаблоны',
                            icon: 'pi pi-file',
                            command: () => router.push('/admin/templates'),
                            visible: auth.user?.roles?.includes('Administrator'),
                        },
                        {
                            label: 'Процессы',
                            icon: 'pi pi-sync',
                            command: () => router.push('/admin/processes'),
                            visible: auth.user?.roles?.includes('Administrator'),
                        },
                    ]
                },
        ]">
            <template #start>
                <span @click="router.push('/documents')" class="layout-logo text-xl font-bold ml-2 mr-2 cursor-pointer">AgroFlow</span>
            </template>
            <template #end>
                <div class="flex items-center gap-2 cursor-pointer" @click="toggleMenu">
                    <Avatar 
                        :image="auth.user?.avatarPath ? http.defaults.baseURL.replace('/api', '') + '/' + auth.user?.avatarPath 
                        : 'https://primefaces.org/cdn/primevue/images/avatar/amyelsner.png'"
                        shape="circle"
                        size="large"
                        class="border-2 border-blue-500"
                    />
                    <span class="font-bold m-auto">{{ auth.user?.displayName || 'Профиль' }}</span>
                </div>
                <Menu ref="menuProfile" :model="menuItems" :popup="true" />
            </template>
        </Menubar>
        <div class="layout-content flex flex-1">
            <aside class="layout-sidebar surface-50 border-right-1 border-gray-200 flex flex-column"
                    :class="{ hidden: !sidebarVisible && isMobile }">
                <PanelMenu :model="slideMenu" class="flex-1 overflow-auto" />
            </aside>
            <main class="layout-main flex-1 overflow-auto p-4">
                <router-view />
            </main>
        </div>
    </div>
</template>

<style scoped>
main {
    padding: 0.5em !important;
}
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