import { PriceCard } from '@/components/chatbot/cards/PriceCard'
import { ListingCard } from '@/components/chatbot/cards/ListingCard'
import type { ChatCard } from '@/api/types'

export function ChatCardList({ cards }: { cards: ChatCard[] }) {
  return (
    <div className="mt-2 flex flex-col gap-2">
      {cards.map((card, index) =>
        card.type === 'price' ? (
          <PriceCard key={`price-${card.productId}-${index}`} card={card} />
        ) : (
          <ListingCard key={`listing-${card.listingId}-${index}`} card={card} />
        )
      )}
    </div>
  )
}
