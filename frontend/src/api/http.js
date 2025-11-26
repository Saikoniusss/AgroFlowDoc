import axios from 'axios';
import router from '../routers';

const http = axios.create({
  baseURL: 'http://localhost:8080/api',
});

const token = localStorage.getItem('token');
if (token) {
  http.defaults.headers.common['Authorization'] = `Bearer ${token}`;
}

http.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      router.push('/login');
    }
    if (error.response && error.response.status === 500) {
      router.push('/server-error');
    }
    return Promise.reject(error);
  }
);

export default http;