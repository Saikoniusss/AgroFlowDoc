import { createRouter, createWebHistory } from "vue-router";

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
            path: '/admin/users',
            component: () => import('../views/AdminPage/AdminUserConfirm.vue'),
            meta: {requiresAdmin: true} 
        },
        { 
            path: '/profile',
            component: () => import('../views/Users/UserProfile.vue')
        },
        {
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
        },
    ]
});

export default router;