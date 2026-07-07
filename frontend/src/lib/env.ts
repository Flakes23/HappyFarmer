function required(name: keyof ImportMetaEnv): string {
  const value = import.meta.env[name]
  if (!value) {
    throw new Error(`Missing required env var: ${name}. Check your .env file (see .env.example).`)
  }
  return value
}

export const env = {
  apiGatewayUrl: required('VITE_API_GATEWAY_URL'),
}
