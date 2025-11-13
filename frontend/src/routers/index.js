import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from '@/store/auth';

const requireAdmin = (to, from, next) => {
    const auth = useAuthStore();
    if (auth.user?.roles?.includes('Administrator')) {
        next();
    } else {
        next('/documents');
    }
}

const requireAuth = (to, from, next) => {
    const auth = useAuthStore();
    if (auth.token) {
        next();
    } else {
        next('/login');
    }
}

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { 
            path: '/',
            redirect: '/login' 
        },
        { 
            path: '/login',
            component: () => import('../views/Users/LoginView.vue')
        },
        { 
            path: '/register',
            component: () => import('../views/Users/RegisterView.vue')
        },
        {
            path: '/',
            component: () => import('../layouts/Layout.vue'),
            children: [
                {
                    path: '/',
                    beforeEnter: requireAuth,
                    children: [
                        {
                            path: '/profile',
                            name: 'Profile',
                            component: () => import('../views/Users/UserProfile.vue')
                        },
                        {
                            path: '/documents',
                            name: 'DocumentList',
                            component: () => import('../views/Document/DocumentList.vue')
                        },
                        {
                            path: '/application',
                            name: 'Application',
                            component: () => import('../views/Application.vue')
                        },
                        {
                            path: '/invoice',
                            name: 'Invoice',
                            component: () => import('../views/Invoice.vue')
                        },
                        {
                            path: '/receipt-request',
                            name: 'Receipt Request',
                            component: () => import('../views/ReceiptRequest.vue')
                        },
                        {
                            path: '/issue-request',
                            name: 'Issue Request',
                            component: () => import('../views/IssueRequest.vue')
                        },
                        {
                            path: '/export-expense-invoice',
                            name: 'Export Expense Invoice',
                            component: () => import('../views/ExportExpenseInvoice.vue')
                        },
                        {
                            path: '/export-shipments-archive',
                            name: 'Export Shipments Archive',
                            component: () => import('../views/ExportShipmentsArchive.vue')
                        },
                        {
                            path: '/warehouse',
                            name: 'Warehouse',
                            component: () => import('../views/Warehouse.vue')
                        },
                        {
                            path: '/grain-reception',
                            name: 'Grain Reception',
                            component: () => import('../views/GrainReception.vue')
                        }
                    ]
                },
                {
                    path: '/admin',
                    beforeEnter: requireAdmin,
                    children: [
                        {
                            path: 'users',
                            component: () => import('../views/AdminPage/AdminUsers.vue'),
                        },
                        {
                            path: 'roles',
                            component: () => import('../views/AdminPage/AdminRoles.vue'),
                        },
                        {
                            path: 'templates',
                            component: () => import('../views/AdminWorkFlow/TemplatesView.vue'),
                        },
                        {
                            path: 'routes',
                            component: () => import('../views/AdminWorkFlow/RoutesView.vue'),
                        },
                        {
                            path: 'processes',
                            component: () => import('../views/AdminWorkFlow/ProcessesView.vue'),
                        }
                    ]
                },
                {
                    path: '/documents',
                    children: [
                        {
                        path: '',
                        name: 'documents',
                        component: () => import('@/views/document/DocumentList.vue')
                        },
                        {
                        path: 'create/:processId',
                        name: 'document-create',
                        component: () => import('@/views/document/DocumentCreate.vue'),
                        props: true
                        }
                    ]
                },
            ]
        },
        {
            path: '/:catchAll(.*)',
            name: 'NotFound',
            component: () => import('../views/Errors/NotFound.vue'),
        },
    ]
});

export default router;