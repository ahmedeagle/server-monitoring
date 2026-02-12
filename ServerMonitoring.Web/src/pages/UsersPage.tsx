import { Box, Typography, Card, CardContent } from '@mui/material'

const UsersPage = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        User Management
      </Typography>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            User Management (Admin Only)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            This feature allows administrators to manage user accounts, roles, and permissions.
            Implementation includes user creation, editing, deletion, and role assignment.
          </Typography>
          <Box mt={2}>
            <Typography variant="subtitle2">Test Users:</Typography>
            <Typography variant="body2">• Admin: admin / Admin@123 (Admin role)</Typography>
            <Typography variant="body2">• User: user / User@123 (User role)</Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default UsersPage
