import { useState, useEffect } from 'react'
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  CircularProgress,
  Switch,
  FormControlLabel,
} from '@mui/material'
import { CheckCircle, Cancel } from '@mui/icons-material'
import { alertService, Alert } from '@/services/alertService'
import { format } from 'date-fns'

const AlertsPage = () => {
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [loading, setLoading] = useState(true)
  const [showOnlyUnacknowledged, setShowOnlyUnacknowledged] = useState(false)

  useEffect(() => {
    loadAlerts()
  }, [showOnlyUnacknowledged])

  const loadAlerts = async () => {
    try {
      const data = await alertService.getAll(showOnlyUnacknowledged)
      setAlerts(data)
    } catch (error) {
      console.error('Failed to load alerts:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleAcknowledge = async (id: number) => {
    try {
      await alertService.acknowledge(id)
      loadAlerts()
    } catch (error) {
      console.error('Failed to acknowledge alert:', error)
    }
  }

  const handleResolve = async (id: number) => {
    try {
      await alertService.resolve(id)
      loadAlerts()
    } catch (error) {
      console.error('Failed to resolve alert:', error)
    }
  }

  const getSeverityColor = (severity: string): "error" | "warning" | "info" | "success" => {
    switch (severity) {
      case 'Critical':
        return 'error'
      case 'Error':
        return 'error'
      case 'Warning':
        return 'warning'
      case 'Info':
        return 'info'
      default:
        return 'info'
    }
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    )
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Alerts</Typography>
        <FormControlLabel
          control={
            <Switch
              checked={showOnlyUnacknowledged}
              onChange={(e) => setShowOnlyUnacknowledged(e.target.checked)}
            />
          }
          label="Unacknowledged Only"
        />
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Severity</TableCell>
              <TableCell>Server</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Value</TableCell>
              <TableCell>Time</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {alerts.map((alert) => (
              <TableRow key={alert.id}>
                <TableCell>
                  <Chip label={alert.severity} color={getSeverityColor(alert.severity)} size="small" />
                </TableCell>
                <TableCell>{alert.serverName}</TableCell>
                <TableCell>{alert.type}</TableCell>
                <TableCell>{alert.title}</TableCell>
                <TableCell>
                  {alert.actualValue?.toFixed(1)}% / {alert.thresholdValue}%
                </TableCell>
                <TableCell>{format(new Date(alert.createdAt), 'PPpp')}</TableCell>
                <TableCell>
                  {alert.isResolved ? (
                    <Chip label="Resolved" color="success" size="small" />
                  ) : alert.isAcknowledged ? (
                    <Chip label="Acknowledged" color="info" size="small" />
                  ) : (
                    <Chip label="New" color="warning" size="small" />
                  )}
                </TableCell>
                <TableCell>
                  {!alert.isAcknowledged && (
                    <IconButton size="small" onClick={() => handleAcknowledge(alert.id)} title="Acknowledge">
                      <CheckCircle color="primary" />
                    </IconButton>
                  )}
                  {!alert.isResolved && (
                    <IconButton size="small" onClick={() => handleResolve(alert.id)} title="Resolve">
                      <Cancel color="success" />
                    </IconButton>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}

export default AlertsPage
