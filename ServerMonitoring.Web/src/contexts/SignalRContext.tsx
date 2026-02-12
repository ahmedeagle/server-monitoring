import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '@/store/authStore'

interface SignalRContextType {
  connection: signalR.HubConnection | null
  isConnected: boolean
}

const SignalRContext = createContext<SignalRContextType>({
  connection: null,
  isConnected: false,
})

export const useSignalR = () => useContext(SignalRContext)

export const SignalRProvider = ({ children }: { children: ReactNode }) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const { token, isAuthenticated } = useAuthStore()

  useEffect(() => {
    if (!isAuthenticated || !token) {
      if (connection) {
        connection.stop()
        setConnection(null)
        setIsConnected(false)
      }
      return
    }

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/monitoring', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build()

    newConnection
      .start()
      .then(() => {
        console.log('SignalR Connected')
        setIsConnected(true)
      })
      .catch((err) => console.error('SignalR Connection Error:', err))

    newConnection.onreconnecting(() => {
      console.log('SignalR Reconnecting...')
      setIsConnected(false)
    })

    newConnection.onreconnected(() => {
      console.log('SignalR Reconnected')
      setIsConnected(true)
    })

    newConnection.onclose(() => {
      console.log('SignalR Disconnected')
      setIsConnected(false)
    })

    setConnection(newConnection)

    return () => {
      newConnection.stop()
    }
  }, [isAuthenticated, token])

  return (
    <SignalRContext.Provider value={{ connection, isConnected }}>
      {children}
    </SignalRContext.Provider>
  )
}
