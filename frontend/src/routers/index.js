import { createRouter, createWebHistory } from "vue-router";
import AdminUsers from '@/views/AdminPage/AdminUsers.vue';
import AdminRoles from '@/views/AdminPage/AdminRoles.vue';
import { useAuthStore } from '@/store/auth';

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
            path: '/profile',
            component: () => import('../views/Users/UserProfile.vue')
        },
        {
        path: '/admin/users',
        component: AdminUsers,
        meta: { requiresAuth: true, requiresAdmin: true },
        },
        {
        path: '/admin/roles',
        component: AdminRoles,
        meta: { requiresAuth: true, requiresAdmin: true },
        },
        {
        path: '/admin/templates',
        component: () => import('@/views/AdminWorkFlow/TemplatesView.vue'), meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
        path: '/admin/routes',
        component: () => import('@/views/AdminWorkFlow/RoutesView.vue'), meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
        path: '/admin/processes',
        component: () => import('@/views/AdminWorkFlow/ProcessesView.vue'), meta: { requiresAuth: true, requiresAdmin: true }
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

router.beforeEach((to, from, next) => {
  const auth = useAuthStore();

  if (to.meta.requiresAuth && !auth.token) {
    next('/login');
    return;
  }

  if (to.meta.requiresAdmin && !auth.user?.roles?.includes('Administrator')) {
    next('/documents');
    return next('/login')
  }

  if (to.meta.requiresAuth) {
    if (!auth.token) {
      return next('/login')
    }
  }
  next();
});

export default router;