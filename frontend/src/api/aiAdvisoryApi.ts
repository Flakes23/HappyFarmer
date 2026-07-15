import { httpAiAdvisory } from '@/api/httpAiAdvisory'
import type {
  ChatMessageDto,
  ChatSessionSummaryResponse,
  CreateChatSessionResponse,
  CreateHarvestPredictionRequest,
  DailyForecastSummaryDto,
  HarvestPredictionResponse,
  HarvestPredictionSummaryDto,
  SendChatMessageResponse,
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
}
