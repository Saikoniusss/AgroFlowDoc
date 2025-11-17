import { defineStore } from 'pinia';
import http from '@/api/http';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
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
        localStorage.setItem('user', JSON.stringify(this.user));
        http.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;
      } catch (err) {
        this.token = null;
        this.user = null;
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        delete http.defaults.headers.common['Authorization'];
        this.error = err.response?.data?.message || 'Ошибка входа';
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
       async fetchMe() {
      if (!this.token) return
      axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`
      try {
        const res = await axios.get(import.meta.env.VITE_API_BASE_URL + '/auth/me')
        this.user = res.data
      } catch {
        this.logout()
      }
    },
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('token');
      delete http.defaults.headers.common['Authorization'];
      window.location.href = "/login"
    },
  },
});