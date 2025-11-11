import axios from 'axios';

const http = axios.create({
  baseURL: 'http://localhost:5097/api',
});

const token = localStorage.getItem('token');
if (token) {
  http.defaults.headers.common['Authorization'] = `Bearer ${token}`;
}

export default http;