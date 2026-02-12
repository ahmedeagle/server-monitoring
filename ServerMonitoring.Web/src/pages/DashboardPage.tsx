import { useState, useEffect, useCallback, useMemo } from 'react'
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  CircularProgress,
} from '@mui/material'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Storage, Warning, CheckCircle, Error } from '@mui/icons-material'
import { serverService, Server, Metric } from '@/services/serverService'
import { alertService, Alert } from '@/services/alertService'
import { useSignalR } from '@/contexts/SignalRContext'
import { format } from 'date-fns'

const DashboardPage = () => {
  const [servers, setServers] = useState<Server[]>([])
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [metrics, setMetrics] = useState<Metric[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedServer, setSelectedServer] = useState<number | null>(null)
  const { connection, isConnected } = useSignalR()

  useEffect(() => {
    loadData()
  }, [])

  // Memoized callback for loading data
  const loadData = useCallback(async () => {
    try {
      const [serversData, alertsData] = await Promise.all([
        serverService.getAll(),
        alertService.getAll(true),
      ])
      setServers(serversData)
      setAlerts(alertsData)

      if (serversData.length > 0) {
        const firstServerId = serversData[0].id
        setSelectedServer(firstServerId)
        const metricsData = await serverService.getMetrics(firstServerId, 50)
        setMetrics(metricsData.reverse())
      }
    } catch (error) {
      console.error('Failed to load dashboard data:', error)
    } finally {
      setLoading(false)
    }
  }, [])

  // Memoized callback for server selection
  const handleServerSelect = useCallback(async (serverId: number) => {
    setSelectedServer(serverId)
    const metricsData = await serverService.getMetrics(serverId, 50)
    setMetrics(metricsData.reverse())
  }, [])

  useEffect(() => {
    if (connection) {
      const handleMetricUpdate = (data: Metric) => {
        if (selectedServer === data.serverId) {
          setMetrics((prev) => [...prev.slice(-50), data])
        }
      }

      const handleAlert = (alert: Alert) => {
        setAlerts((prev) => [alert, ...prev])
      }

      connection.on('ReceiveMetricUpdate', handleMetricUpdate)
      connection.on('ReceiveAlert', handleAlert)

      return () => {
        connection.off('ReceiveMetricUpdate', handleMetricUpdate)
        connection.off('ReceiveAlert', handleAlert)
      }
    }
  }, [connection, selectedServer])

  // Memoized chart data calculation
  const chartData = useMemo(() => {
    return metrics.map((m) => {
      const date = new Date(m.recordedAt)
      const time = isNaN(date.getTime()) ? 'N/A' : format(date, 'HH:mm:ss')
      return {
        time,
        CPU: m.cpuUsage,
        Memory: m.memoryUsage,
        Disk: m.diskUsage,
      }
    })
  }, [metrics])

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    )
  }

  const activeServers = servers.filter((s) => s.isActive).length
  const totalAlerts = alerts.length
  const criticalAlerts = alerts.filter((a) => a.severity === 'Critical').length

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <Chip
          icon={isConnected ? <CheckCircle /> : <Error />}
          label={isConnected ? 'Connected' : 'Disconnected'}
          color={isConnected ? 'success' : 'error'}
          variant="outlined"
        />
      </Box>

      <Grid container spacing={3}>
        {/* Summary Cards */}
        <Grid item xs={12} sm={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <div>
                  <Typography color="text.secondary" gutterBottom>
                    Active Servers
                  </Typography>
                  <Typography variant="h4">{activeServers}/{servers.length}</Typography>
                </div>
                <Storage sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <div>
                  <Typography color="text.secondary" gutterBottom>
                    Total Alerts
                  </Typography>
                  <Typography variant="h4">{totalAlerts}</Typography>
                </div>
                <Warning sx={{ fontSize: 40, color: 'warning.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <div>
                  <Typography color="text.secondary" gutterBottom>
                    Critical Alerts
                  </Typography>
                  <Typography variant="h4">{criticalAlerts}</Typography>
                </div>
                <Error sx={{ fontSize: 40, color: 'error.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Server Selection */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Select Server
              </Typography>
              <Box display="flex" gap={1} flexWrap="wrap">
                {servers.map((server) => (
                  <Chip
                    key={server.id}
                    label={server.name}
                    onClick={() => handleServerSelect(server.id)}
                    color={selectedServer === server.id ? 'primary' : 'default'}
                    variant={selectedServer === server.id ? 'filled' : 'outlined'}
                  />
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Real-time Charts */}
        {selectedServer && chartData.length > 0 && (
          <>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    CPU, Memory & Disk Usage (%)
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={chartData}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="time" />
                      <YAxis domain={[0, 100]} />
                      <Tooltip />
                      <Legend />
                      <Line type="monotone" dataKey="CPU" stroke="#8884d8" strokeWidth={2} />
                      <Line type="monotone" dataKey="Memory" stroke="#82ca9d" strokeWidth={2} />
                      <Line type="monotone" dataKey="Disk" stroke="#ffc658" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </>
        )}

        {/* Recent Alerts */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Alerts
              </Typography>
              {alerts.length === 0 ? (
                <Typography color="text.secondary">No alerts</Typography>
              ) : (
                <Box>
                  {alerts.slice(0, 5).map((alert) => (
                    <Box key={alert.id} py={1} borderBottom={1} borderColor="divider">
                      <Box display="flex" alignItems="center" gap={1}>
                        <Chip label={alert.severity} size="small" color={
                          alert.severity === 'Critical' ? 'error' :
                          alert.severity === 'Warning' ? 'warning' : 'info'
                        } />
                        <Typography variant="body2">{alert.title}</Typography>
                      </Box>
                      <Typography variant="caption" color="text.secondary">
                        {alert.serverName} • {(() => {
                          const date = new Date(alert.createdAt)
                          return isNaN(date.getTime()) ? 'Unknown time' : format(date, 'PPpp')
                        })()}
                      </Typography>
                    </Box>
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default DashboardPage
