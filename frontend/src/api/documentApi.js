import axios from 'axios'
import { useAuthStore } from '@/store/auth'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL + '/documents'
})

// 🔐 Добавляем токен автоматически
api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  return config
})

export default {
  // Получить список доступных процессов (видов документов)
  getProcesses() {
    return api.get('/processes')
  },

  // Получить процесс + шаблон + шаги маршрута
  getProcessDetails(processId) {
    return api.get(`/process/${processId}`)
  },

  // Создать документ
  createDocument(payload) {
    return api.post('/create', payload)
  },

  // Получить документ (для просмотра)
  getDocument(docId) {
    return api.get(`/${docId}`)
  },

  // Обновить документ (черновик)
  updateDocument(docId, payload) {
    return api.put(`/${docId}`, payload)
  },

  // Отправить на согласование
  sendToApprove(docId) {
    return api.post(`/${docId}/send`)
  },

  // Утвердить документ
  approveDocument(docId) {
    return api.post(`/${docId}/approve`)
  },

  // Отклонить документ
  rejectDocument(docId, comment) {
    return api.post(`/${docId}/reject`, { comment })
  },

  // Добавить комментарий
  addComment(docId, comment) {
    return api.post(`/${docId}/comment`, { comment })
  },

  // Загрузка файлов (позже настроим BE)
  uploadFile(docId, file) {
    const form = new FormData()
    form.append('file', file)
    return api.post(`/${docId}/file`, form)
  }
}