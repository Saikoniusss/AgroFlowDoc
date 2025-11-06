import { defineStore } from 'pinia';
import http from '@/api/http';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    loading: false,
    error: null,
  }),
  actions: {
    async login(username, password) {
      this.loading = true;
      this.error = null;
      try {
        const res = await http.post('/auth/login', { username, password });
        this.user = res.data;
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка входа';
      } finally {
        this.loading = false;
      }
    },
    async register(data) {
      this.loading = true;
      this.error = null;
      try {
        const res = await http.post('/auth/register', data);
        return res.data;
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка регистрации';
        throw err;
      } finally {
        this.loading = false;
      }
    },
    async fetchUser() {
      try {
        const res = await http.get('/auth/me');
        this.user = res.data;
      } catch {
        this.user = null;
      }
    },
    async logout() {
      await http.post('/auth/logout');
      this.user = null;
    },
  },
});