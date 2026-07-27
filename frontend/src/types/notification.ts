export interface NotificationItem {
  id: string
  companyId: string
  userId: string
  title: string
  message: string
  category: string
  priority: string
  status: string
  sourceEntity: string
  sourceEntityId?: string | null
  createdAt: string
  readAt?: string | null
  archived: boolean
  navigationPath?: string | null
}

export interface NotificationUnreadCount {
  unreadCount: number
}

export interface NotificationQuery {
  companyId?: string
  userId?: string
  category?: string
  priority?: string
  status?: string
  archived?: boolean
  sourceEntity?: string
  search?: string
  orderBy?: string
  orderDirection?: string
}
