import { useState, useEffect } from 'react'
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  CircularProgress,
} from '@mui/material'
import { Add, Download } from '@mui/icons-material'
import { reportService, Report } from '@/services/reportService'
import { format } from 'date-fns'

const ReportsPage = () => {
  const [reports, setReports] = useState<Report[]>([])
  const [loading, setLoading] = useState(true)
  const [openDialog, setOpenDialog] = useState(false)
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    type: 'DailyMetrics',
    startDate: format(new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd'),
    endDate: format(new Date(), 'yyyy-MM-dd'),
  })

  useEffect(() => {
    loadReports()
  }, [])

  const loadReports = async () => {
    try {
      const data = await reportService.getAll()
      setReports(data)
    } catch (error) {
      console.error('Failed to load reports:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleOpenDialog = () => {
    setFormData({
      title: '',
      description: '',
      type: 'DailyMetrics',
      startDate: format(new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd'),
      endDate: format(new Date(), 'yyyy-MM-dd'),
    })
    setOpenDialog(true)
  }

  const handleCloseDialog = () => {
    setOpenDialog(false)
  }

  const handleSubmit = async () => {
    try {
      await reportService.generate(formData)
      handleCloseDialog()
      loadReports()
    } catch (error) {
      console.error('Failed to generate report:', error)
    }
  }

  const handleDownload = async (id: number, title: string) => {
    try {
      const blob = await reportService.download(id)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `${title}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Failed to download report:', error)
    }
  }

  const getStatusColor = (status: string): "success" | "error" | "warning" | "info" => {
    switch (status) {
      case 'Completed':
        return 'success'
      case 'Failed':
        return 'error'
      case 'Processing':
        return 'warning'
      case 'Pending':
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
        <Typography variant="h4">Reports</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={handleOpenDialog}>
          Generate Report
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Title</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Period</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Generated At</TableCell>
              <TableCell>Size</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {reports.map((report) => (
              <TableRow key={report.id}>
                <TableCell>{report.title}</TableCell>
                <TableCell>{report.type}</TableCell>
                <TableCell>
                  {format(new Date(report.startDate), 'PP')} - {format(new Date(report.endDate), 'PP')}
                </TableCell>
                <TableCell>
                  <Chip label={report.status} color={getStatusColor(report.status)} size="small" />
                </TableCell>
                <TableCell>
                  {report.generatedAt ? format(new Date(report.generatedAt), 'PPpp') : '-'}
                </TableCell>
                <TableCell>
                  {report.fileSizeBytes ? `${(report.fileSizeBytes / 1024).toFixed(0)} KB` : '-'}
                </TableCell>
                <TableCell>
                  {report.status === 'Completed' && (
                    <IconButton size="small" onClick={() => handleDownload(report.id, report.title)}>
                      <Download />
                    </IconButton>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Report</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            margin="normal"
            label="Title"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="Description"
            multiline
            rows={3}
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="Report Type"
            select
            value={formData.type}
            onChange={(e) => setFormData({ ...formData, type: e.target.value })}
          >
            <MenuItem value="DailyMetrics">Daily Metrics</MenuItem>
            <MenuItem value="WeeklyMetrics">Weekly Metrics</MenuItem>
            <MenuItem value="MonthlyMetrics">Monthly Metrics</MenuItem>
            <MenuItem value="CustomPeriod">Custom Period</MenuItem>
            <MenuItem value="AlertSummary">Alert Summary</MenuItem>
            <MenuItem value="ServerHealth">Server Health</MenuItem>
          </TextField>
          <TextField
            fullWidth
            margin="normal"
            label="Start Date"
            type="date"
            value={formData.startDate}
            onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            fullWidth
            margin="normal"
            label="End Date"
            type="date"
            value={formData.endDate}
            onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
            InputLabelProps={{ shrink: true }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSubmit} variant="contained">
            Generate
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default ReportsPage
