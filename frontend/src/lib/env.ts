function required(name: keyof ImportMetaEnv): string {
  const value = import.meta.env[name]
  if (!value) {
    throw new Error(`Missing required env var: ${name}. Check your .env file (see .env.example).`)
  }
  return value
}

export const env = {
  authApiUrl: required('VITE_AUTH_API_URL'),
  marketPriceApiUrl: required('VITE_MARKET_PRICE_API_URL'),
}
