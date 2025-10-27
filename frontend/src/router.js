import { createRouter, createWebHistory } from "vue-router";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/application',
            name: 'Application',
            component: () => import('./components/views/Application.vue')
        },
        {
            path: '/invoice',
            name: 'Invoice',
            component: () => import('./components/views/Invoice.vue')
        },
        {
            path: '/receipt-request',
            name: 'Receipt Request',
            component: () => import('./components/views/ReceiptRequest.vue')
        },
        {
            path: '/issue-request',
            name: 'Issue Request',
            component: () => import('./components/views/IssueRequest.vue')
        },
        {
            path: '/export-expense-invoice',
            name: 'Export Expense Invoice',
            component: () => import('./components/views/ExportExpenseInvoice.vue')
        },
        {
            path: '/export-shipments-archive',
            name: 'Export Shipments Archive',
            component: () => import('./components/views/ExportShipmentsArchive.vue')
        },
        {
            path: '/warehouse',
            name: 'Warehouse',
            component: () => import('./components/views/Warehouse.vue')
        },
        {
            path: '/grain-reception',
            name: 'Grain Reception',
            component: () => import('./components/views/GrainReception.vue')
        }
    ]
});

export default router;