import { DEFAULT_COMPANY_ID } from './theme'

const STORAGE_KEY = 'agencyos.activeCompanyId'

/** Reads the active Company id (US-402), defaulting to the seeded default Company. */
export function getActiveCompanyId(): string {
  if (typeof window === 'undefined') {
    return DEFAULT_COMPANY_ID
  }

  return window.localStorage.getItem(STORAGE_KEY) || DEFAULT_COMPANY_ID
}

export function setActiveCompanyId(companyId: string): void {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(STORAGE_KEY, companyId)
}
