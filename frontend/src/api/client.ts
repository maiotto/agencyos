import axios from 'axios'
import { getActiveCompanyId } from '../companyStorage'
import { getMyWorkUserId } from '../myWorkStorage'

export const apiClient = axios.create({
  baseURL: '/',
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const companyId = getActiveCompanyId()
  if (companyId) {
    config.headers['X-Company-Id'] = companyId
  }

  // User identity for My Work Dashboard and Notification Center (US-501 / US-506 / DEC-501-001).
  const userId = getMyWorkUserId()
  if (userId) {
    config.headers['X-User-Id'] = userId
  }

  return config
})

export function getErrorMessage(error: unknown, fallback = 'An unexpected error occurred.'): string {
  if (!axios.isAxiosError(error)) {
    return fallback
  }

  const data = error.response?.data as
    | { detail?: string; title?: string; errors?: Record<string, string[]> }
    | undefined

  if (data?.errors) {
    const messages = Object.values(data.errors).flat()
    if (messages.length > 0) {
      return messages.join(' ')
    }
  }

  if (data?.detail) {
    return data.detail
  }

  if (data?.title) {
    return data.title
  }

  return fallback
}
