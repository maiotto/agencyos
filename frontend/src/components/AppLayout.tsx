import {
  AppBar,
  Badge,
  Box,
  Button,
  Container,
  FormControl,
  MenuItem,
  Select,
  Toolbar,
  Typography,
} from '@mui/material'
import { Link as RouterLink, useLocation } from 'react-router-dom'
import { useEffect, useState, type ReactNode } from 'react'
import { useCompany } from '../context/CompanyContext'
import { getUnreadNotificationCount } from '../api/notifications'
import { getMyWorkUserId } from '../myWorkStorage'

interface AppLayoutProps {
  children: ReactNode
}

export function AppLayout({ children }: AppLayoutProps) {
  const location = useLocation()
  const { activeCompanyId, companies, setActiveCompanyId } = useCompany()
  const [unreadCount, setUnreadCount] = useState(0)

  useEffect(() => {
    let cancelled = false
    void (async () => {
      try {
        const result = await getUnreadNotificationCount({
          userId: getMyWorkUserId() || 'system',
        })
        if (!cancelled) {
          setUnreadCount(result.unreadCount)
        }
      } catch {
        if (!cancelled) {
          setUnreadCount(0)
        }
      }
    })()
    return () => {
      cancelled = true
    }
  }, [location.pathname, activeCompanyId])

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background:
          'radial-gradient(circle at top left, rgba(15, 76, 92, 0.14), transparent 42%), linear-gradient(180deg, #F2F6F8 0%, #D9E4E9 100%)',
      }}
    >
      <AppBar
        position="sticky"
        color="transparent"
        sx={{
          backdropFilter: 'blur(10px)',
          backgroundColor: 'rgba(255, 255, 255, 0.88)',
          borderBottom: '1px solid rgba(15, 76, 92, 0.12)',
          boxShadow: 'none',
        }}
      >
        <Toolbar sx={{ gap: 2, flexWrap: 'wrap' }}>
          <Typography
            component={RouterLink}
            to="/enterprise-dashboard"
            variant="h6"
            sx={{
              color: 'primary.main',
              textDecoration: 'none',
              fontFamily: '"IBM Plex Serif", Georgia, serif',
              fontWeight: 600,
            }}
          >
            AgencyOS
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mr: 1 }}>
            Administrative Configuration
          </Typography>
          <FormControl size="small" sx={{ minWidth: 180, mr: 1 }}>
            <Select
              value={companies.some((company) => company.id === activeCompanyId) ? activeCompanyId : ''}
              onChange={(event) => setActiveCompanyId(event.target.value)}
              displayEmpty
              renderValue={(value) => {
                const selected = companies.find((company) => company.id === value)
                return selected ? selected.companyName : 'Select company'
              }}
            >
              {companies.map((company) => (
                <MenuItem key={company.id} value={company.id}>
                  {company.companyName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            component={RouterLink}
            to="/enterprise-dashboard"
            color={location.pathname === '/enterprise-dashboard' || location.pathname === '/' ? 'primary' : 'inherit'}
            variant={location.pathname === '/enterprise-dashboard' || location.pathname === '/' ? 'outlined' : 'text'}
            size="small"
          >
            Dashboard
          </Button>
          <Button
            component={RouterLink}
            to="/executive-workspace"
            color={location.pathname.startsWith('/executive-workspace') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/executive-workspace') ? 'outlined' : 'text'}
            size="small"
          >
            Executive Workspace
          </Button>
          <Button
            component={RouterLink}
            to="/my-work"
            color={location.pathname.startsWith('/my-work') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/my-work') ? 'outlined' : 'text'}
            size="small"
          >
            My Work
          </Button>
          <Button
            component={RouterLink}
            to="/notifications"
            color={location.pathname.startsWith('/notifications') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/notifications') ? 'outlined' : 'text'}
            size="small"
          >
            <Badge badgeContent={unreadCount} color="error" max={99} sx={{ pr: unreadCount > 0 ? 1.5 : 0 }}>
              Notifications
            </Badge>
          </Button>
          <Button
            component={RouterLink}
            to="/personal-dashboard"
            color={location.pathname.startsWith('/personal-dashboard') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/personal-dashboard') ? 'outlined' : 'text'}
            size="small"
          >
            Productivity
          </Button>
          <Button
            component={RouterLink}
            to="/planning-workspace"
            color={location.pathname.startsWith('/planning-workspace') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/planning-workspace') ? 'outlined' : 'text'}
            size="small"
          >
            Planning Workspace
          </Button>
          <Button
            component={RouterLink}
            to="/companies"
            color={location.pathname.startsWith('/companies') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/companies') ? 'outlined' : 'text'}
            size="small"
          >
            Companies
          </Button>
          <Button
            component={RouterLink}
            to="/working-calendars"
            color={location.pathname.startsWith('/working-calendars') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/working-calendars') ? 'outlined' : 'text'}
            size="small"
          >
            Working Calendars
          </Button>
          <Button
            component={RouterLink}
            to="/holidays"
            color={location.pathname.startsWith('/holidays') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/holidays') ? 'outlined' : 'text'}
            size="small"
          >
            Holidays
          </Button>
          <Button
            component={RouterLink}
            to="/working-hours"
            color={location.pathname.startsWith('/working-hours') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/working-hours') ? 'outlined' : 'text'}
            size="small"
          >
            Working Hours
          </Button>
          <Button
            component={RouterLink}
            to="/resource-availabilities"
            color={location.pathname.startsWith('/resource-availabilities') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/resource-availabilities') ? 'outlined' : 'text'}
            size="small"
          >
            Resource Availability
          </Button>
          <Button
            component={RouterLink}
            to="/capacity"
            color={location.pathname === '/capacity' ? 'primary' : 'inherit'}
            variant={location.pathname === '/capacity' ? 'outlined' : 'text'}
            size="small"
          >
            Capacity
          </Button>
          <Button
            component={RouterLink}
            to="/capacity/history"
            color={location.pathname.startsWith('/capacity/history') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/capacity/history') ? 'outlined' : 'text'}
            size="small"
          >
            Capacity History
          </Button>
          <Button
            component={RouterLink}
            to="/workload/history"
            color={location.pathname.startsWith('/workload/history') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/workload/history') ? 'outlined' : 'text'}
            size="small"
          >
            Workload History
          </Button>
          <Button
            component={RouterLink}
            to="/planning-templates"
            color={location.pathname.startsWith('/planning-templates') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/planning-templates') ? 'outlined' : 'text'}
            size="small"
          >
            Planning Templates
          </Button>
          <Button
            component={RouterLink}
            to="/portfolios"
            color={location.pathname.startsWith('/portfolios') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/portfolios') ? 'outlined' : 'text'}
            size="small"
          >
            Portfolios
          </Button>
          <Button
            component={RouterLink}
            to="/portfolio-analytics"
            color={location.pathname.startsWith('/portfolio-analytics') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/portfolio-analytics') ? 'outlined' : 'text'}
            size="small"
          >
            Portfolio Analytics
          </Button>
          <Button
            component={RouterLink}
            to="/cross-portfolio-planning"
            color={location.pathname.startsWith('/cross-portfolio-planning') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/cross-portfolio-planning') ? 'outlined' : 'text'}
            size="small"
          >
            Cross-Portfolio Planning
          </Button>
          <Button
            component={RouterLink}
            to="/recommendation-workspace"
            color={location.pathname.startsWith('/recommendation-workspace') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/recommendation-workspace') ? 'outlined' : 'text'}
            size="small"
          >
            Recommendation Workspace
          </Button>
          <Button
            component={RouterLink}
            to="/recommendations"
            color={
              location.pathname.startsWith('/recommendations') &&
              !location.pathname.startsWith('/recommendations/workflow') &&
              !location.pathname.startsWith('/recommendations/history') &&
              !location.pathname.startsWith('/recommendations/compare')
                ? 'primary'
                : 'inherit'
            }
            variant={
              location.pathname.startsWith('/recommendations') &&
              !location.pathname.startsWith('/recommendations/workflow') &&
              !location.pathname.startsWith('/recommendations/history') &&
              !location.pathname.startsWith('/recommendations/compare')
                ? 'outlined'
                : 'text'
            }
            size="small"
          >
            Recommendations
          </Button>
          <Button
            component={RouterLink}
            to="/recommendations/history"
            color={location.pathname.startsWith('/recommendations/history') ? 'primary' : 'inherit'}
            variant={
              location.pathname.startsWith('/recommendations/history') ? 'outlined' : 'text'
            }
            size="small"
          >
            Rec. History
          </Button>
          <Button
            component={RouterLink}
            to="/recommendations/compare"
            color={location.pathname.startsWith('/recommendations/compare') ? 'primary' : 'inherit'}
            variant={
              location.pathname.startsWith('/recommendations/compare') ? 'outlined' : 'text'
            }
            size="small"
          >
            Rec. Compare
          </Button>
          <Button
            component={RouterLink}
            to="/recommendations/workflow"
            color={location.pathname.startsWith('/recommendations/workflow') ? 'primary' : 'inherit'}
            variant={
              location.pathname.startsWith('/recommendations/workflow') ? 'outlined' : 'text'
            }
            size="small"
          >
            Approval Workflow
          </Button>
          <Button
            component={RouterLink}
            to="/decisions"
            color={
              location.pathname.startsWith('/decisions') && !location.pathname.startsWith('/decision-workspace')
                ? 'primary'
                : 'inherit'
            }
            variant={
              location.pathname.startsWith('/decisions') && !location.pathname.startsWith('/decision-workspace')
                ? 'outlined'
                : 'text'
            }
            size="small"
          >
            Decisions
          </Button>
          <Button
            component={RouterLink}
            to="/decision-workspace"
            color={location.pathname.startsWith('/decision-workspace') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/decision-workspace') ? 'outlined' : 'text'}
            size="small"
          >
            Decision Workspace
          </Button>
          <Button
            component={RouterLink}
            to="/decision-profiles"
            color={location.pathname.startsWith('/decision-profiles') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/decision-profiles') ? 'outlined' : 'text'}
            size="small"
          >
            Decision Profiles
          </Button>
          <Button
            component={RouterLink}
            to="/ai-recommendations"
            color={location.pathname.startsWith('/ai-recommendations') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/ai-recommendations') ? 'outlined' : 'text'}
            size="small"
          >
            AI Recs
          </Button>
          <Button
            component={RouterLink}
            to="/explainability"
            color={location.pathname.startsWith('/explainability') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/explainability') ? 'outlined' : 'text'}
            size="small"
          >
            Explainability
          </Button>
          <Button
            component={RouterLink}
            to="/executive-summaries"
            color={location.pathname.startsWith('/executive-summaries') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/executive-summaries') ? 'outlined' : 'text'}
            size="small"
          >
            Exec Summaries
          </Button>
          <Button
            component={RouterLink}
            to="/audit"
            color={location.pathname.startsWith('/audit') ? 'primary' : 'inherit'}
            variant={location.pathname.startsWith('/audit') ? 'outlined' : 'text'}
            size="small"
          >
            Audit
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: { xs: 3, md: 5 } }}>
        {children}
      </Container>
    </Box>
  )
}
