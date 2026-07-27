import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { listCompanies, selectCompany } from '../api/companies'
import { getActiveCompanyId, setActiveCompanyId as persistActiveCompanyId } from '../companyStorage'
import type { Company } from '../types/company'

interface CompanyContextValue {
  activeCompanyId: string
  companies: Company[]
  loading: boolean
  error: string | null
  setActiveCompanyId: (companyId: string) => void
  refreshCompanies: () => Promise<void>
}

const CompanyContext = createContext<CompanyContextValue | undefined>(undefined)

export function CompanyProvider({ children }: { children: ReactNode }) {
  const [activeCompanyId, setActiveCompanyIdState] = useState(getActiveCompanyId)
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const refreshCompanies = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const data = await listCompanies({ status: 'Active', orderBy: 'companyName' })
      setCompanies(data)
    } catch {
      setError('Failed to load companies.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void refreshCompanies()
  }, [refreshCompanies])

  const setActiveCompanyId = useCallback((companyId: string) => {
    persistActiveCompanyId(companyId)
    setActiveCompanyIdState(companyId)
    // Best-effort: attempts to select the Company server-side (BR-2003). Every subsequent
    // request already carries the X-Company-Id header regardless of this call's outcome.
    void selectCompany(companyId).catch(() => undefined)
  }, [])

  const value = useMemo(
    () => ({ activeCompanyId, companies, loading, error, setActiveCompanyId, refreshCompanies }),
    [activeCompanyId, companies, loading, error, setActiveCompanyId, refreshCompanies],
  )

  return <CompanyContext.Provider value={value}>{children}</CompanyContext.Provider>
}

export function useCompany(): CompanyContextValue {
  const context = useContext(CompanyContext)
  if (!context) {
    throw new Error('useCompany must be used within a CompanyProvider.')
  }

  return context
}
