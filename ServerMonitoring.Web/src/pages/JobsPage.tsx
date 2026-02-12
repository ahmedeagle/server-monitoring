import { Box, Typography, Card, CardContent, Button } from '@mui/material'
import { OpenInNew } from '@mui/icons-material'

const JobsPage = () => {
  const handleOpenHangfire = () => {
    window.open('http://localhost:5000/hangfire', '_blank')
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Background Jobs
      </Typography>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Hangfire Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            View and manage background jobs using the Hangfire dashboard. Monitor recurring jobs,
            fire-and-forget jobs, delayed jobs, and their execution history.
          </Typography>
          <Button variant="contained" startIcon={<OpenInNew />} onClick={handleOpenHangfire}>
            Open Hangfire Dashboard
          </Button>
        </CardContent>
      </Card>

      <Box mt={3}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Configured Jobs
            </Typography>
            <Box mt={2}>
              <Typography variant="subtitle2" gutterBottom>
                📊 Metrics Collection Job
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Runs every 5 minutes to collect system metrics from all active servers
              </Typography>

              <Typography variant="subtitle2" gutterBottom>
                ⚠️ Alert Processing Job
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Runs every 2 minutes to check metric thresholds and generate alerts
              </Typography>

              <Typography variant="subtitle2" gutterBottom>
                📄 Report Generation Job
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Fire-and-forget job triggered when users request report generation
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  )
}

export default JobsPage
