import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL + '/documents'
})

const token = localStorage.getItem('token');
if (token) {
  api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
}


export default {
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

  uploadFile(documentId, file) {
    const formData = new FormData()
    formData.append("file", file)

    return api.post(`/documents/${documentId}/files/upload`, formData, {
      headers: { "Content-Type": "multipart/form-data" }
    })
  },
  getMenuCounts() {
    return api.get('/menu-counts')
  },
  getMyDocuments() {
    return api.get('/my')
  },
  getTodoDocuments() {
    return api.get('/todo')
  },
  getArchive() {
    return api.get('/archive')
  }
}