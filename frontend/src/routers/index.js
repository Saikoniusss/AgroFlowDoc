import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '@/views/Users/LoginView.vue';
import RegisterView from '@/views/Users/RegisterView.vue';
import DocumentList from '@/views/Document/DocumentList.vue';
import AdminUserConfirm from '@/views/AdminPage/AdminUserConfirm.vue';
import ProfileView from '@/views/Users/UserProfile.vue';

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', component: LoginView },
  { path: '/register', component: RegisterView },
  { path: '/documents', component: DocumentList },
  { path: '/admin/users', component: AdminUserConfirm, meta: {requiresAdmin: true} },
  { path: '/profile', component: ProfileView },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;