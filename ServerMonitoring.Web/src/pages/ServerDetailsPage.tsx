import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  CircularProgress,
  Chip,
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
  AreaChart,
  Area,
} from 'recharts'
import { serverService, Server, Metric } from '@/services/serverService'
import { format } from 'date-fns'

const ServerDetailsPage = () => {
  const { id } = useParams<{ id: string }>()
  const [server, setServer] = useState<Server | null>(null)
  const [metrics, setMetrics] = useState<Metric[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (id) {
      loadServerDetails(parseInt(id))
    }
  }, [id])

  const loadServerDetails = async (serverId: number) => {
    try {
      const [serverData, metricsData] = await Promise.all([
        serverService.getById(serverId),
        serverService.getMetrics(serverId, 100),
      ])
      setServer(serverData)
      setMetrics(metricsData.reverse())
    } catch (error) {
      console.error('Failed to load server details:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    )
  }

  if (!server) {
    return <Typography>Server not found</Typography>
  }

  const chartData = metrics.map((m) => {
    const date = new Date(m.recordedAt)
    const time = isNaN(date.getTime()) ? 'N/A' : format(date, 'HH:mm')
    return {
      time,
      CPU: m.cpuUsage,
      Memory: m.memoryUsage,
      Disk: m.diskUsage,
      Network: (m.networkInbound + m.networkOutbound) / 2,
      ResponseTime: m.responseTime,
    }
  })

  const latestMetric = metrics[metrics.length - 1]

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {server.name}
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Server Information
              </Typography>
              <Box>
                <Typography variant="body2"><strong>Hostname:</strong> {server.hostname}</Typography>
                <Typography variant="body2"><strong>IP Address:</strong> {server.ipAddress}</Typography>
                <Typography variant="body2"><strong>Port:</strong> {server.port}</Typography>
                <Typography variant="body2"><strong>OS:</strong> {server.operatingSystem}</Typography>
                <Typography variant="body2">
                  <strong>Status:</strong>{' '}
                  <Chip
                    label={server.isActive ? 'Active' : 'Inactive'}
                    color={server.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {latestMetric && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Current Metrics
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">CPU</Typography>
                    <Typography variant="h5">{latestMetric.cpuUsage.toFixed(1)}%</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Memory</Typography>
                    <Typography variant="h5">{latestMetric.memoryUsage.toFixed(1)}%</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Disk</Typography>
                    <Typography variant="h5">{latestMetric.diskUsage.toFixed(1)}%</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Response Time</Typography>
                    <Typography variant="h5">{latestMetric.responseTime.toFixed(0)}ms</Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        )}

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                CPU, Memory & Disk Usage History
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <AreaChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="time" />
                  <YAxis domain={[0, 100]} />
                  <Tooltip />
                  <Legend />
                  <Area type="monotone" dataKey="CPU" stackId="1" stroke="#8884d8" fill="#8884d8" />
                  <Area type="monotone" dataKey="Memory" stackId="2" stroke="#82ca9d" fill="#82ca9d" />
                  <Area type="monotone" dataKey="Disk" stackId="3" stroke="#ffc658" fill="#ffc658" />
                </AreaChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Response Time History
              </Typography>
              <ResponsiveContainer width="100%" height={200}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="time" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="ResponseTime" stroke="#ff7300" strokeWidth={2} />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default ServerDetailsPage
