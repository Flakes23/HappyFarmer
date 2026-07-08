import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { env } from '@/lib/env'
import { useAuthStore } from '@/store/authStore'
import { appendIfNew } from '@/hooks/mutations/useSendMessage'
import type { MessageHistoryResponse, MessageResponse } from '@/api/types'

const ChatConnectionContext = createContext<HubConnection | null>(null)

export function useChatConnection() {
  return useContext(ChatConnectionContext)
}

export function ChatConnectionProvider({ children }: { children: ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const queryClient = useQueryClient()
  const [connection, setConnection] = useState<HubConnection | null>(null)
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!isAuthenticated) {
      connectionRef.current?.stop()
      connectionRef.current = null
      setConnection(null)
      return
    }

    const conn = new HubConnectionBuilder()
      .withUrl(`${env.apiGatewayUrl}/api/marketplace/hubs/chat`, {
        accessTokenFactory: () => useAuthStore.getState().accessToken ?? '',
        // SignalR's IHttpConnectionOptions defaults withCredentials to true (cookie-based auth
        // assumption). We authenticate via bearer token only, and the Gateway's CORS policy has
        // no AllowCredentials() — a credentialed cross-origin request would be silently rejected
        // by the browser (the connection fails with no visible error). Must be false here.
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    conn.on('ReceiveMessage', (msg: MessageResponse) => {
      queryClient.setQueryData<MessageHistoryResponse>(['messages', msg.interestId], (old) => appendIfNew(old, msg))
      queryClient.invalidateQueries({ queryKey: ['my-interests'] })
    })

    // React StrictMode (dev only) mount → cleanup → mount lại effect này 1 lần để phát hiện
    // side-effect thiếu dọn dẹp. Nếu cleanup gọi conn.stop() ngay trong lúc start() còn đang
    // negotiate, SignalR sẽ ném lỗi "stopped during negotiation" — vô hại nhưng ồn ào. Thay vì
    // stop() ngay trong cleanup, đánh dấu `cancelled` và chỉ stop() SAU KHI start() đã settle,
    // tránh hẳn race condition này (áp dụng cả khi auth-state đổi nhanh ở production, không chỉ
    // riêng StrictMode).
    let cancelled = false

    conn
      .start()
      .then(() => {
        if (cancelled) {
          conn.stop()
          return
        }
        connectionRef.current = conn
        setConnection(conn)
      })
      .catch(() => {
        // swallowed: withAutomaticReconnect / the next auth-state change retries the connection
      })

    return () => {
      cancelled = true
      if (connectionRef.current === conn) {
        connectionRef.current = null
        setConnection(null)
      }
      if (conn.state === HubConnectionState.Connected) {
        conn.stop()
      }
    }
  }, [isAuthenticated, queryClient])

  return <ChatConnectionContext.Provider value={connection}>{children}</ChatConnectionContext.Provider>
}
