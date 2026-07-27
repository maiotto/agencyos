import { apiClient } from './client'
import type {
  NotificationItem,
  NotificationQuery,
  NotificationUnreadCount,
} from '../types/notification'

function toParams(query: NotificationQuery = {}) {
  return {
    companyId: query.companyId || undefined,
    userId: query.userId || undefined,
    category: query.category || undefined,
    priority: query.priority || undefined,
    status: query.status || undefined,
    archived: query.archived,
    sourceEntity: query.sourceEntity || undefined,
    search: query.search || undefined,
    orderBy: query.orderBy || undefined,
    orderDirection: query.orderDirection || undefined,
  }
}

export async function listNotifications(query: NotificationQuery = {}): Promise<NotificationItem[]> {
  const { data } = await apiClient.get<NotificationItem[]>('/notifications', {
    params: toParams(query),
  })
  return data
}

export async function filterNotifications(query: NotificationQuery = {}): Promise<NotificationItem[]> {
  const { data } = await apiClient.get<NotificationItem[]>('/notifications/filter', {
    params: toParams(query),
  })
  return data
}

export async function getNotification(id: string): Promise<NotificationItem> {
  const { data } = await apiClient.get<NotificationItem>(`/notifications/${id}`)
  return data
}

export async function getUnreadNotificationCount(
  query: NotificationQuery = {},
): Promise<NotificationUnreadCount> {
  const { data } = await apiClient.get<NotificationUnreadCount>('/notifications/unread-count', {
    params: toParams(query),
  })
  return data
}

export async function markNotificationRead(id: string): Promise<NotificationItem> {
  const { data } = await apiClient.post<NotificationItem>(`/notifications/${id}/read`)
  return data
}

export async function markNotificationUnread(id: string): Promise<NotificationItem> {
  const { data } = await apiClient.post<NotificationItem>(`/notifications/${id}/unread`)
  return data
}

export async function archiveNotification(id: string): Promise<NotificationItem> {
  const { data } = await apiClient.post<NotificationItem>(`/notifications/${id}/archive`)
  return data
}
