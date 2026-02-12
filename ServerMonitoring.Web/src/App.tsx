import { BrowserRouter } from 'react-router-dom'
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material'
import { useState, useMemo } from 'react'
import AppRoutes from './routes'
import { SignalRProvider } from './contexts/SignalRContext'

function App() {
  const [mode, setMode] = useState<'light' | 'dark'>('light')

  const theme = useMemo(
    () =>
      createTheme({
        palette: {
          mode,
          primary: {
            main: '#1976d2',
          },
          secondary: {
            main: '#dc004e',
          },
        },
      }),
    [mode],
  )

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <SignalRProvider>
          <AppRoutes darkMode={mode === 'dark'} toggleDarkMode={() => setMode(mode === 'light' ? 'dark' : 'light')} />
        </SignalRProvider>
      </BrowserRouter>
    </ThemeProvider>
  )
}

export default App
