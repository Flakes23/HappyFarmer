import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ChatbotPage } from '@/pages/ChatbotPage'
import { HarvestPredictionPage } from '@/pages/HarvestPredictionPage'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function AiAdvisoryPage() {
  useDocumentTitle('Tư vấn AI — HappyFarmer')

  return (
    <Tabs defaultValue="chatbot">
      <TabsList>
        <TabsTrigger value="chatbot">Chatbot</TabsTrigger>
        <TabsTrigger value="harvest">Dự đoán thu hoạch</TabsTrigger>
      </TabsList>

      <TabsContent value="chatbot">
        <ChatbotPage />
      </TabsContent>

      <TabsContent value="harvest">
        <HarvestPredictionPage />
      </TabsContent>
    </Tabs>
  )
}
