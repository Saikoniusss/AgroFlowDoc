import { defineStore } from 'pinia';
import http from '@/api/http';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token') || null,
    error: null,
  }),
  actions: {
    async login(username, password) {
      try {
        const res = await http.post('/auth/login', { username, password });
        this.token = res.data.access_token;
        this.user = res.data.user;
        localStorage.setItem('token', this.token);
        http.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка входа';
      }
    },
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('token');
      delete http.defaults.headers.common['Authorization'];
    },
  },
});