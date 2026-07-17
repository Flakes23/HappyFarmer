import { Link } from 'react-router-dom'
import { Bot, LineChart, MessageCircle, Plus, ShoppingBasket, Sprout } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function HomePage() {
  useDocumentTitle('HappyFarmer — Kết nối nông dân và người mua')
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)

  return (
    <div className="space-y-12">
      <section className="flex flex-col items-center gap-6 py-8 text-center">
        <span className="flex h-14 w-14 items-center justify-center rounded-full bg-secondary text-primary">
          <Sprout className="h-7 w-7" />
        </span>
        <div className="space-y-3">
          <h1 className="text-3xl font-semibold text-text sm:text-4xl">
            Kết nối nông dân và người mua trực tiếp
          </h1>
          <p className="mx-auto max-w-xl text-text-muted">
            Tra cứu giá nông sản mỗi ngày, đăng bán hoặc tìm mua nông sản trên Chợ nông sản HappyFarmer —
            không qua trung gian.
          </p>
        </div>

        <div className="flex flex-wrap items-center justify-center gap-3">
          {isAuthenticated ? (
            <>
              {user?.role === 'Farmer' ? (
                <Button size="lg" asChild>
                  <Link to="/marketplace/new">
                    <Plus className="h-4 w-4" />
                    Đăng tin bán
                  </Link>
                </Button>
              ) : null}
              {user?.role === 'Buyer' ? (
                <Button size="lg" asChild>
                  <Link to="/marketplace/buy-requests/new">
                    <Plus className="h-4 w-4" />
                    Đăng yêu cầu mua
                  </Link>
                </Button>
              ) : null}
              <Button size="lg" variant="outline" asChild>
                <Link to="/marketplace">Xem Chợ nông sản</Link>
              </Button>
            </>
          ) : (
            <>
              <Button size="lg" asChild>
                <Link to="/register">Đăng ký ngay</Link>
              </Button>
              <Button size="lg" variant="outline" asChild>
                <Link to="/login">Đăng nhập</Link>
              </Button>
            </>
          )}
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader>
            <LineChart className="h-6 w-6 text-primary" />
            <CardTitle className="pt-2">Giá nông sản</CardTitle>
            <CardDescription>
              Theo dõi giá nông sản theo sản phẩm và khu vực, xem biểu đồ biến động giá theo thời gian.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="link" className="h-auto p-0" asChild>
              <Link to="/prices">Xem giá ngay →</Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <ShoppingBasket className="h-6 w-6 text-primary" />
            <CardTitle className="pt-2">Chợ nông sản</CardTitle>
            <CardDescription>
              Đăng bán nông sản kèm ảnh, hoặc tìm mua từ nông dân khắp cả nước — liên hệ trực tiếp.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="link" className="h-auto p-0" asChild>
              <Link to="/marketplace">Vào chợ ngay →</Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <MessageCircle className="h-6 w-6 text-primary" />
            <CardTitle className="pt-2">Liên hệ trực tiếp</CardTitle>
            <CardDescription>
              Trò chuyện trực tiếp với người bán/người mua ngay trên nền tảng, không cần qua trung gian.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="link" className="h-auto p-0" asChild>
              <Link to={isAuthenticated ? '/marketplace/my-interests' : '/register'}>
                {isAuthenticated ? 'Xem liên hệ của tôi →' : 'Đăng ký để bắt đầu →'}
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <Bot className="h-6 w-6 text-primary" />
            <CardTitle className="pt-2">Tư vấn AI</CardTitle>
            <CardDescription>
              Chatbot tư vấn canh tác, dự đoán thời điểm thu hoạch và nhận diện bệnh cây qua ảnh —
              có AI đồng hành cùng bạn.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="link" className="h-auto p-0" asChild>
              <Link to={isAuthenticated ? '/tu-van-ai' : '/register'}>
                {isAuthenticated ? 'Trải nghiệm ngay →' : 'Đăng ký để bắt đầu →'}
              </Link>
            </Button>
          </CardContent>
        </Card>
      </section>
    </div>
  )
}
