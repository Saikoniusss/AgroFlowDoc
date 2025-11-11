import axios from 'axios'
import { useAuthStore } from '@/store/auth'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL + '/admin/workflow'
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
  // Templates
  getTemplates() {
    return api.get('/templates')
  },
  createTemplate(data) {
    console.log(data)
    return api.post('/templates', data)
  },
  addField(templateId, data) {
    return api.post(`/templates/${templateId}/fields`, data)
  },

  // Routes
  getRoutes() {
    return api.get('/routes')
  },
  createRoute(data) {
    return api.post('/routes', data)
  },
  addStep(routeId, data) {
    return api.post(`/routes/${routeId}/steps`, data)
  },

  // Processes
  getProcesses() {
    return api.get('/processes')
  },
  createProcess(data) {
    return api.post('/processes', data)
  },
    // 👥 Users & Roles
  getUsers: () => api.get('/users'),
  getRoles: () => api.get('/roles'),
  updateStep: (routeId, stepId, data) => api.put(`/routes/${routeId}/steps/${stepId}`, data),
  updateTemplate: (id, data) => api.put(`/templates/${id}`, data),
  updateField: (templateId, fieldId, data) => api.put(`/templates/${templateId}/fields/${fieldId}`, data),
  deleteField: (templateId, fieldId) => api.delete(`/templates/${templateId}/fields/${fieldId}`),
  updateFieldOrder(templateId, fields) {
    return api.put(`/templates/${templateId}/fields/reorder`, fields)
  },
  updateRouteStepOrder(routeId, steps) {
  return api.put(`/routes/${routeId}/steps/reorder`, steps)
  },
}