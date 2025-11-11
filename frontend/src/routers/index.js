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
            beforeEnter: requireAuth,
            children: [
                {
                    path: '/profile',
                    name: 'Profile',
                    component: () => import('../views/Users/UserProfile.vue')
                },
                {
                    path: '/',
                    component: () => import('../layouts/Layout.vue'),
                    children: [
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
                }
            ]
        },
        {
            beforeEnter: requireAdmin,
            children: [
                {
                    path: '/',
                    component: () => import('../layouts/Layout.vue'),
                    children: [
                        {
                            path: '/admin/users',
                            component: () => import('../views/AdminPage/AdminUsers.vue'),
                        },
                        {
                            path: '/admin/roles',
                            component: () => import('../views/AdminPage/AdminRoles.vue'),
                        },
                    ]
                }
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