import axios from 'axios';

const http = axios.create({
  baseURL: 'http://localhost:5097/api',
  withCredentials: true, // важно для cookie
});

export default http;