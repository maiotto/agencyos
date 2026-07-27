import { useCallback, useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Badge,
  Box,
  Button,
  Chip,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import {
  archiveNotification,
  filterNotifications,
  getNotification,
  getUnreadNotificationCount,
  markNotificationRead,
  markNotificationUnread,
} from '../api/notifications'
import { getErrorMessage } from '../api/client'
import type { NotificationItem } from '../types/notification'
import { getMyWorkUserId } from '../myWorkStorage'

const CATEGORIES = [
  '',
  'Recommendation',
  'Decision',
  'Capacity',
  'Portfolio',
  'Audit',
  'Planning',
  'Executive',
  'Company',
  'System',
]

const PRIORITIES = ['', 'Low', 'Medium', 'High', 'Critical']
const STATUSES = ['', 'Unread', 'Read']

function priorityColor(priority: string): 'default' | 'success' | 'warning' | 'error' | 'info' {
  switch (priority) {
    case 'Critical':
      return 'error'
    case 'High':
      return 'warning'
    case 'Medium':
      return 'info'
    case 'Low':
      return 'success'
    default:
      return 'default'
  }
}

export function NotificationCenterPage() {
  const navigate = useNavigate()
  const { id } = useParams<{ id?: string }>()
  const [category, setCategory] = useState('')
  const [priority, setPriority] = useState('')
  const [status, setStatus] = useState('')
  const [archived, setArchived] = useState<'active' | 'archived' | 'all'>('active')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<NotificationItem[]>([])
  const [selected, setSelected] = useState<NotificationItem | null>(null)
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const userId = getMyWorkUserId() || 'system'

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const query = {
        userId,
        category: category || undefined,
        priority: priority || undefined,
        status: status || undefined,
        archived: archived === 'all' ? undefined : archived === 'archived',
        search: search || undefined,
        orderBy: 'createdAt',
        orderDirection: 'desc',
      }
      const [list, count] = await Promise.all([
        filterNotifications(query),
        getUnreadNotificationCount({ userId }),
      ])
      setItems(list)
      setUnreadCount(count.unreadCount)
    } catch (err) {
      setItems([])
      setError(getErrorMessage(err, 'Failed to load notifications.'))
    } finally {
      setLoading(false)
    }
  }, [archived, category, priority, search, status, userId])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (!id) {
      setSelected(null)
      return
    }

    let cancelled = false
    void (async () => {
      try {
        const item = await getNotification(id)
        if (!cancelled) {
          setSelected(item)
        }
      } catch (err) {
        if (!cancelled) {
          setSelected(null)
          setError(getErrorMessage(err, 'Failed to load notification detail.'))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [id])

  async function onMarkRead(notificationId: string) {
    await markNotificationRead(notificationId)
    await load()
    if (selected?.id === notificationId) {
      setSelected(await getNotification(notificationId))
    }
  }

  async function onMarkUnread(notificationId: string) {
    await markNotificationUnread(notificationId)
    await load()
    if (selected?.id === notificationId) {
      setSelected(await getNotification(notificationId))
    }
  }

  async function onArchive(notificationId: string) {
    await archiveNotification(notificationId)
    await load()
    if (selected?.id === notificationId) {
      navigate('/notifications')
    }
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Stack direction="row" spacing={2} alignItems="center" useFlexGap flexWrap="wrap">
          <Typography variant="h4">Notification Center</Typography>
          <Badge badgeContent={unreadCount} color="error" max={99}>
            <Chip label="Unread" color={unreadCount > 0 ? 'error' : 'default'} size="small" />
          </Badge>
        </Stack>
        <Typography color="text.secondary">
          Centralized in-app notifications for pending actions and operational events (US-506).
          Informational only — never modifies business data.
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Viewing as user: {userId}
        </Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} useFlexGap flexWrap="wrap">
          <TextField
            select
            label="Category"
            size="small"
            value={category}
            onChange={(event) => setCategory(event.target.value)}
            sx={{ minWidth: 160 }}
          >
            {CATEGORIES.map((value) => (
              <MenuItem key={value || 'all'} value={value}>
                {value || 'All categories'}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Priority"
            size="small"
            value={priority}
            onChange={(event) => setPriority(event.target.value)}
            sx={{ minWidth: 140 }}
          >
            {PRIORITIES.map((value) => (
              <MenuItem key={value || 'all'} value={value}>
                {value || 'All priorities'}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Status"
            size="small"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
            sx={{ minWidth: 140 }}
          >
            {STATUSES.map((value) => (
              <MenuItem key={value || 'all'} value={value}>
                {value || 'All statuses'}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Archive"
            size="small"
            value={archived}
            onChange={(event) => setArchived(event.target.value as 'active' | 'archived' | 'all')}
            sx={{ minWidth: 140 }}
          >
            <MenuItem value="active">Active</MenuItem>
            <MenuItem value="archived">Archived</MenuItem>
            <MenuItem value="all">All</MenuItem>
          </TextField>
          <TextField
            label="Search"
            size="small"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ minWidth: 200 }}
          />
          <Button variant="outlined" onClick={() => void load()}>
            Refresh
          </Button>
        </Stack>
      </Paper>

      <Stack direction={{ xs: 'column', lg: 'row' }} spacing={2} alignItems="stretch">
        <Paper sx={{ flex: 2, p: 1, minWidth: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress size={28} />
            </Box>
          ) : (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Title</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Priority</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Created</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {items.map((item) => (
                    <TableRow
                      key={item.id}
                      hover
                      selected={selected?.id === item.id}
                      sx={{
                        cursor: 'pointer',
                        fontWeight: item.status === 'Unread' ? 700 : 400,
                        backgroundColor:
                          item.status === 'Unread' ? 'rgba(15, 76, 92, 0.06)' : undefined,
                      }}
                      onClick={() => navigate(`/notifications/${item.id}`)}
                    >
                      <TableCell>
                        <Typography variant="body2" fontWeight={item.status === 'Unread' ? 700 : 500}>
                          {item.title}
                        </Typography>
                      </TableCell>
                      <TableCell>{item.category}</TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={item.priority}
                          color={priorityColor(item.priority)}
                        />
                      </TableCell>
                      <TableCell>{item.archived ? 'Archived' : item.status}</TableCell>
                      <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                  {items.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5}>
                        <Typography color="text.secondary">No notifications match the filters.</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </Paper>

        <Paper sx={{ flex: 1, p: 2, minWidth: 280 }}>
          <Typography variant="h6" gutterBottom>
            Notification Detail
          </Typography>
          {!selected ? (
            <Typography color="text.secondary">Select a notification to view details.</Typography>
          ) : (
            <Stack spacing={1.5}>
              <Typography variant="subtitle1" fontWeight={700}>
                {selected.title}
              </Typography>
              <Typography variant="body2">{selected.message}</Typography>
              <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
                <Chip size="small" label={selected.category} />
                <Chip
                  size="small"
                  label={selected.priority}
                  color={priorityColor(selected.priority)}
                />
                <Chip size="small" label={selected.archived ? 'Archived' : selected.status} />
              </Stack>
              <Typography variant="body2" color="text.secondary">
                Source: {selected.sourceEntity}
                {selected.sourceEntityId ? ` / ${selected.sourceEntityId}` : ''}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Created: {new Date(selected.createdAt).toLocaleString()}
              </Typography>
              {selected.readAt && (
                <Typography variant="body2" color="text.secondary">
                  Read: {new Date(selected.readAt).toLocaleString()}
                </Typography>
              )}
              <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
                {selected.status === 'Unread' ? (
                  <Button size="small" variant="contained" onClick={() => void onMarkRead(selected.id)}>
                    Mark read
                  </Button>
                ) : (
                  <Button size="small" variant="outlined" onClick={() => void onMarkUnread(selected.id)}>
                    Mark unread
                  </Button>
                )}
                {!selected.archived && (
                  <Button size="small" color="warning" onClick={() => void onArchive(selected.id)}>
                    Archive
                  </Button>
                )}
                {selected.navigationPath && (
                  <Button
                    size="small"
                    component={RouterLink}
                    to={selected.navigationPath}
                    variant="outlined"
                  >
                    Open source
                  </Button>
                )}
              </Stack>
            </Stack>
          )}
        </Paper>
      </Stack>
    </Stack>
  )
}
