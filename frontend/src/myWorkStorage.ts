const USER_ID_STORAGE_KEY = 'agencyos.myWork.userId'

/**
 * Reads the caller's UserId used for the My Work Dashboard (US-501 / DEC-501-001).
 * There is no full auth identity provider yet, so this is a simple, explicit override:
 * the value is sent both as the `userId` query parameter and as the `X-User-Id` header,
 * matching the backend's `UserId ?? IAuditContext.UserId ?? "system"` resolution order.
 */
export function getMyWorkUserId(): string {
  if (typeof window === 'undefined') {
    return ''
  }

  return window.localStorage.getItem(USER_ID_STORAGE_KEY) || ''
}

export function setMyWorkUserId(userId: string): void {
  if (typeof window === 'undefined') {
    return
  }

  if (userId) {
    window.localStorage.setItem(USER_ID_STORAGE_KEY, userId)
  } else {
    window.localStorage.removeItem(USER_ID_STORAGE_KEY)
  }
}
