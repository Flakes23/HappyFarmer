import { httpAiAdvisory } from '@/api/httpAiAdvisory'
import type {
  ChatMessageDto,
  ChatSessionSummaryResponse,
  CreateChatSessionResponse,
  CreateDiseaseDetectionRequest,
  CreateHarvestPredictionRequest,
  DailyForecastSummaryDto,
  DiseaseDetectionResponse,
  DiseaseDetectionSummaryDto,
  HarvestPredictionResponse,
  HarvestPredictionSummaryDto,
  SendChatMessageResponse,
  UploadSignatureResponse,
} from '@/api/types'

export const aiAdvisoryApi = {
  listSessions: () =>
    httpAiAdvisory.get<ChatSessionSummaryResponse[]>('/api/ai-advisory/chat/sessions').then((r) => r.data),

  createSession: () =>
    httpAiAdvisory.post<CreateChatSessionResponse>('/api/ai-advisory/chat/sessions').then((r) => r.data),

  deleteSession: (id: number) =>
    httpAiAdvisory.delete(`/api/ai-advisory/chat/sessions/${id}`).then((r) => r.data),

  getHistory: (sessionId: number) =>
    httpAiAdvisory.get<ChatMessageDto[]>(`/api/ai-advisory/chat/sessions/${sessionId}/messages`).then((r) => r.data),

  sendMessage: (sessionId: number, message: string) =>
    httpAiAdvisory
      .post<SendChatMessageResponse>(`/api/ai-advisory/chat/sessions/${sessionId}/messages`, { message })
      .then((r) => r.data),

  predictHarvest: (payload: CreateHarvestPredictionRequest) =>
    httpAiAdvisory
      .post<HarvestPredictionResponse>('/api/ai-advisory/harvest-prediction', payload)
      .then((r) => r.data),

  getHarvestHistory: () =>
    httpAiAdvisory
      .get<HarvestPredictionSummaryDto[]>('/api/ai-advisory/harvest-prediction/history')
      .then((r) => r.data),

  getHarvestPredictionDetail: (id: number) =>
    httpAiAdvisory.get<HarvestPredictionResponse>(`/api/ai-advisory/harvest-prediction/${id}`).then((r) => r.data),

  deleteHarvestPrediction: (id: number) =>
    httpAiAdvisory.delete(`/api/ai-advisory/harvest-prediction/${id}`).then((r) => r.data),

  getWeatherForecast: (location: string) =>
    httpAiAdvisory
      .get<DailyForecastSummaryDto[]>('/api/ai-advisory/harvest-prediction/weather-forecast', { params: { location } })
      .then((r) => r.data),

  getDiseaseDetectionSignature: () =>
    httpAiAdvisory
      .get<UploadSignatureResponse>('/api/ai-advisory/disease-detection/cloudinary-signature')
      .then((r) => r.data),

  detectDisease: (payload: CreateDiseaseDetectionRequest) =>
    httpAiAdvisory.post<DiseaseDetectionResponse>('/api/ai-advisory/disease-detection', payload).then((r) => r.data),

  getDiseaseHistory: () =>
    httpAiAdvisory
      .get<DiseaseDetectionSummaryDto[]>('/api/ai-advisory/disease-detection/history')
      .then((r) => r.data),

  getDiseaseDetectionDetail: (id: number) =>
    httpAiAdvisory.get<DiseaseDetectionResponse>(`/api/ai-advisory/disease-detection/${id}`).then((r) => r.data),

  deleteDiseaseDetection: (id: number) =>
    httpAiAdvisory.delete(`/api/ai-advisory/disease-detection/${id}`).then((r) => r.data),
}

export interface InvalidPlantImageDetail {
  message: string
  description: string
}

/** Tách body 422 `{ message, description }` của disease-detection, trả null nếu error không đúng hình dạng này. */
export function extractInvalidPlantImage(error: unknown): InvalidPlantImageDetail | null {
  if (typeof error !== 'object' || error === null || !('response' in error)) return null
  const response = (error as { response?: { status?: number; data?: Record<string, unknown> } }).response
  if (response?.status !== 422) return null
  const data = response.data
  const message = typeof data?.message === 'string' ? data.message : 'Ảnh không hợp lệ.'
  const description = typeof data?.description === 'string' ? data.description : ''
  return { message, description }
}
