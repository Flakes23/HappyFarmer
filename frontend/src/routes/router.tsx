import { lazy } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { RootLayout } from '@/layouts/RootLayout'
import { RequireAuth } from '@/components/auth/RequireAuth'
import { RouteErrorBoundary } from '@/components/RouteErrorBoundary'
import { HomePage } from '@/pages/HomePage'

const LoginPage = lazy(() => import('@/pages/LoginPage').then((m) => ({ default: m.LoginPage })))
const RegisterPage = lazy(() => import('@/pages/RegisterPage').then((m) => ({ default: m.RegisterPage })))
const ForgotPasswordPage = lazy(() =>
  import('@/pages/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage })),
)
const MarketPricePage = lazy(() => import('@/pages/MarketPricePage').then((m) => ({ default: m.MarketPricePage })))
const ProductPriceDetailPage = lazy(() =>
  import('@/pages/ProductPriceDetailPage').then((m) => ({ default: m.ProductPriceDetailPage })),
)
const MarketplacePage = lazy(() => import('@/pages/MarketplacePage').then((m) => ({ default: m.MarketplacePage })))
const ListingDetailPage = lazy(() =>
  import('@/pages/ListingDetailPage').then((m) => ({ default: m.ListingDetailPage })),
)
const BuyRequestDetailPage = lazy(() =>
  import('@/pages/BuyRequestDetailPage').then((m) => ({ default: m.BuyRequestDetailPage })),
)
const NewListingPage = lazy(() => import('@/pages/NewListingPage').then((m) => ({ default: m.NewListingPage })))
const MyListingsPage = lazy(() => import('@/pages/MyListingsPage').then((m) => ({ default: m.MyListingsPage })))
const NewBuyRequestPage = lazy(() =>
  import('@/pages/NewBuyRequestPage').then((m) => ({ default: m.NewBuyRequestPage })),
)
const MyInterestsPage = lazy(() => import('@/pages/MyInterestsPage').then((m) => ({ default: m.MyInterestsPage })))
const InterestThreadPage = lazy(() =>
  import('@/pages/InterestThreadPage').then((m) => ({ default: m.InterestThreadPage })),
)
const EditProfilePage = lazy(() =>
  import('@/pages/EditProfilePage').then((m) => ({ default: m.EditProfilePage })),
)
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage').then((m) => ({ default: m.NotFoundPage })))

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
      {
        path: 'profile',
        element: (
          <RequireAuth>
            <EditProfilePage />
          </RequireAuth>
        ),
      },
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
