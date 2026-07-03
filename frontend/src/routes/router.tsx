import { createBrowserRouter, Navigate } from 'react-router-dom'
import { RootLayout } from '@/layouts/RootLayout'
import { LoginPage } from '@/pages/LoginPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { MarketPricePage } from '@/pages/MarketPricePage'
import { ProductPriceDetailPage } from '@/pages/ProductPriceDetailPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    children: [
      { index: true, element: <Navigate to="/prices" replace /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      { path: 'prices', element: <MarketPricePage /> },
      { path: 'prices/:productId', element: <ProductPriceDetailPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
