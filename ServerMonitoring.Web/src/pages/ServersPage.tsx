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
    setOpenDialog(true)
  }, [])

  // Memoized callback for closing dialog
  const handleCloseDialog = useCallback(() => {
    setOpenDialog(false)
    setEditingServer(null)
  }, [])

  // Memoized callback for form submission
  const handleSubmit = useCallback(async () => {
    try {
      if (editingServer) {
        await serverService.update(editingServer.id, formData)
      } else {
        await serverService.create(formData)
      }
      handleCloseDialog()
      loadServers()
    } catch (error) {
      console.error('Failed to save server:', error)
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
          <TextField
            fullWidth
            margin="normal"
            label="Name"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="Hostname"
            value={formData.hostname}
            onChange={(e) => setFormData({ ...formData, hostname: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="IP Address"
            value={formData.ipAddress}
            onChange={(e) => setFormData({ ...formData, ipAddress: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="Port"
            type="number"
            value={formData.port}
            onChange={(e) => setFormData({ ...formData, port: parseInt(e.target.value) })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="Operating System"
            select
            value={formData.operatingSystem}
            onChange={(e) => setFormData({ ...formData, operatingSystem: e.target.value })}
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
