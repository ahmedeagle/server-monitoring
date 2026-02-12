import { useState, useEffect, useCallback } from 'react'
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
  Alert,
} from '@mui/material'
import { Add, Edit, Delete, Visibility } from '@mui/icons-material'
import { serverService, Server } from '@/services/serverService'
import { useNavigate } from 'react-router-dom'

const ServersPage = () => {
  const [servers, setServers] = useState<Server[]>([])
  const [loading, setLoading] = useState(true)
  const [openDialog, setOpenDialog] = useState(false)
  const [editingServer, setEditingServer] = useState<Server | null>(null)
  const [formData, setFormData] = useState({
    name: '',
    hostname: '',
    ipAddress: '',
    port: 22,
    operatingSystem: 'Linux',
    isActive: true,
  })
  const [formErrors, setFormErrors] = useState({
    name: '',
    hostname: '',
    ipAddress: '',
    port: '',
  })
  const [submitError, setSubmitError] = useState('')
  const navigate = useNavigate()

  // Memoized callback for loading servers
  const loadServers = useCallback(async () => {
    try {
      const data = await serverService.getAll()
      setServers(data)
    } catch (error) {
      console.error('Failed to load servers:', error)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadServers()
  }, [loadServers])

  // Validation function
  const validateForm = () => {
    const errors = {
      name: '',
      hostname: '',
      ipAddress: '',
      port: '',
    }
    let isValid = true

    if (!formData.name.trim()) {
      errors.name = 'Server name is required'
      isValid = false
    } else if (formData.name.length > 200) {
      errors.name = 'Server name must not exceed 200 characters'
      isValid = false
    }

    if (!formData.hostname.trim()) {
      errors.hostname = 'Hostname is required'
      isValid = false
    } else if (formData.hostname.length > 200) {
      errors.hostname = 'Hostname must not exceed 200 characters'
      isValid = false
    }

    if (!formData.ipAddress.trim()) {
      errors.ipAddress = 'IP Address is required'
      isValid = false
    } else if (!/^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/.test(formData.ipAddress)) {
      errors.ipAddress = 'Invalid IP Address format (e.g., 192.168.1.1)'
      isValid = false
    }

    if (formData.port < 1 || formData.port > 65535) {
      errors.port = 'Port must be between 1 and 65535'
      isValid = false
    }

    setFormErrors(errors)
    return isValid
  }

  // Memoized callback for opening dialog
  const handleOpenDialog = useCallback((server?: Server) => {
    if (server) {
      setEditingServer(server)
      setFormData({
        name: server.name,
        hostname: server.hostname,
        ipAddress: server.ipAddress,
        port: server.port,
        operatingSystem: server.operatingSystem,
        isActive: server.isActive,
      })
    } else {
      setEditingServer(null)
      setFormData({
        name: '',
        hostname: '',
        ipAddress: '',
        port: 22,
        operatingSystem: 'Linux',
        isActive: true,
      })
    }
    // Reset errors when opening dialog
    setFormErrors({
      name: '',
      hostname: '',
      ipAddress: '',
      port: '',
    })
    setSubmitError('')
    setOpenDialog(true)
  }, [])

  // Memoized callback for closing dialog
  const handleCloseDialog = useCallback(() => {
    setOpenDialog(false)
    setEditingServer(null)
  }, [])

  // Memoized callback for form submission
  const handleSubmit = useCallback(async () => {
    // Validate form before submitting
    if (!validateForm()) {
      setSubmitError('Please fix the errors above')
      return
    }

    try {
      setSubmitError('')
      if (editingServer) {
        await serverService.update(editingServer.id, formData)
      } else {
        await serverService.create(formData)
      }
      handleCloseDialog()
      loadServers()
    } catch (error: any) {
      console.error('Failed to save server:', error)
      // Show backend validation errors
      const errorMessage = error.response?.data?.message || 'Failed to save server. Please try again.'
      setSubmitError(errorMessage)
    }
  }, [editingServer, formData, handleCloseDialog, loadServers])

  // Memoized callback for deleting server
  const handleDelete = useCallback(async (id: number) => {
    if (confirm('Are you sure you want to delete this server?')) {
      try {
        await serverService.delete(id)
        loadServers()
      } catch (error) {
        console.error('Failed to delete server:', error)
      }
    }
  }, [loadServers])

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
        <Typography variant="h4">Servers</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenDialog()}>
          Add Server
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Hostname</TableCell>
              <TableCell>IP Address</TableCell>
              <TableCell>OS</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {servers.map((server) => (
              <TableRow key={server.id}>
                <TableCell>{server.name}</TableCell>
                <TableCell>{server.hostname}</TableCell>
                <TableCell>{server.ipAddress}</TableCell>
                <TableCell>{server.operatingSystem}</TableCell>
                <TableCell>
                  <Chip
                    label={server.isActive ? 'Active' : 'Inactive'}
                    color={server.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <IconButton size="small" onClick={() => navigate(`/servers/${server.id}`)}>
                    <Visibility />
                  </IconButton>
                  <IconButton size="small" onClick={() => handleOpenDialog(server)}>
                    <Edit />
                  </IconButton>
                  <IconButton size="small" onClick={() => handleDelete(server.id)}>
                    <Delete />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingServer ? 'Edit Server' : 'Add Server'}</DialogTitle>
        <DialogContent>
          {submitError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {submitError}
            </Alert>
          )}
          <TextField
            fullWidth
            margin="normal"
            label="Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            error={!!formErrors.name}
            helperText={formErrors.name}
            required
          />
          <TextField
            fullWidth
            margin="normal"
            label="Hostname"
            value={formData.hostname}
            onChange={(e) => setFormData({ ...formData, hostname: e.target.value })}
            error={!!formErrors.hostname}
            helperText={formErrors.hostname}
            required
          />
          <TextField
            fullWidth
            margin="normal"
            label="IP Address"
            value={formData.ipAddress}
            onChange={(e) => setFormData({ ...formData, ipAddress: e.target.value })}
            error={!!formErrors.ipAddress}
            helperText={formErrors.ipAddress || 'Format: 192.168.1.1'}
            required
          />
          <TextField
            fullWidth
            margin="normal"
            label="Port"
            type="number"
            value={formData.port}
            onChange={(e) => setFormData({ ...formData, port: parseInt(e.target.value) || 0 })}
            error={!!formErrors.port}
            helperText={formErrors.port}
            required
          />
          <TextField
            fullWidth
            margin="normal"
            label="Operating System"
            select
            value={formData.operatingSystem}
            onChange={(e) => setFormData({ ...formData, operatingSystem: e.target.value })}
            required
          >
            <MenuItem value="Linux">Linux</MenuItem>
            <MenuItem value="Windows">Windows</MenuItem>
            <MenuItem value="MacOS">MacOS</MenuItem>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSubmit} variant="contained">
            {editingServer ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default ServersPage
