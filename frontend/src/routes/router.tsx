import { createBrowserRouter } from 'react-router-dom'
import { RootLayout } from '@/layouts/RootLayout'
import { RequireAuth } from '@/components/auth/RequireAuth'
import { RouteErrorBoundary } from '@/components/RouteErrorBoundary'
import { HomePage } from '@/pages/HomePage'
import { LoginPage } from '@/pages/LoginPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { ForgotPasswordPage } from '@/pages/ForgotPasswordPage'
import { MarketPricePage } from '@/pages/MarketPricePage'
import { ProductPriceDetailPage } from '@/pages/ProductPriceDetailPage'
import { MarketplacePage } from '@/pages/MarketplacePage'
import { ListingDetailPage } from '@/pages/ListingDetailPage'
import { BuyRequestDetailPage } from '@/pages/BuyRequestDetailPage'
import { NewListingPage } from '@/pages/NewListingPage'
import { MyListingsPage } from '@/pages/MyListingsPage'
import { NewBuyRequestPage } from '@/pages/NewBuyRequestPage'
import { MyInterestsPage } from '@/pages/MyInterestsPage'
import { InterestThreadPage } from '@/pages/InterestThreadPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    errorElement: <RouteErrorBoundary />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      { path: 'forgot-password', element: <ForgotPasswordPage /> },
      { path: 'prices', element: <MarketPricePage /> },
      { path: 'prices/:productId', element: <ProductPriceDetailPage /> },
      { path: 'marketplace', element: <MarketplacePage /> },
      { path: 'marketplace/listings/:id', element: <ListingDetailPage /> },
      { path: 'marketplace/buy-requests/:id', element: <BuyRequestDetailPage /> },
      {
        path: 'marketplace/new',
        element: (
          <RequireAuth role="Farmer">
            <NewListingPage />
          </RequireAuth>
        ),
      },
      {
        path: 'marketplace/my-listings',
        element: (
          <RequireAuth role="Farmer">
            <MyListingsPage />
          </RequireAuth>
        ),
      },
      {
        path: 'marketplace/buy-requests/new',
        element: (
          <RequireAuth role="Buyer">
            <NewBuyRequestPage />
          </RequireAuth>
        ),
      },
      {
        path: 'marketplace/my-interests',
        element: (
          <RequireAuth>
            <MyInterestsPage />
          </RequireAuth>
        ),
      },
      {
        path: 'marketplace/my-interests/:id',
        element: (
          <RequireAuth>
            <InterestThreadPage />
          </RequireAuth>
        ),
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
