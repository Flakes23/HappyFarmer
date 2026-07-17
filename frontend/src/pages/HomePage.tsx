import { Link } from 'react-router-dom'
import { Bot, LineChart, MessageCircle, Plus, ShoppingBasket, Sprout, type LucideIcon } from 'lucide-react'
import { motion } from 'framer-motion'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useMotionVariants } from '@/lib/motion'

interface FeatureCard {
  icon: LucideIcon
  title: string
  description: string
  href: string
  cta: string
}

export function HomePage() {
  useDocumentTitle('HappyFarmer — Kết nối nông dân và người mua')
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)
  const { fadeInUp, staggerContainer } = useMotionVariants()

  const features: FeatureCard[] = [
    {
      icon: LineChart,
      title: 'Giá nông sản',
      description: 'Theo dõi giá nông sản theo sản phẩm và khu vực, xem biểu đồ biến động giá theo thời gian.',
      href: '/prices',
      cta: 'Xem giá ngay →',
    },
    {
      icon: ShoppingBasket,
      title: 'Chợ nông sản',
      description: 'Đăng bán nông sản kèm ảnh, hoặc tìm mua từ nông dân khắp cả nước — liên hệ trực tiếp.',
      href: '/marketplace',
      cta: 'Vào chợ ngay →',
    },
    {
      icon: MessageCircle,
      title: 'Liên hệ trực tiếp',
      description: 'Trò chuyện trực tiếp với người bán/người mua ngay trên nền tảng, không cần qua trung gian.',
      href: isAuthenticated ? '/marketplace/my-interests' : '/register',
      cta: isAuthenticated ? 'Xem liên hệ của tôi →' : 'Đăng ký để bắt đầu →',
    },
    {
      icon: Bot,
      title: 'Tư vấn AI',
      description:
        'Chatbot tư vấn canh tác, dự đoán thời điểm thu hoạch và nhận diện bệnh cây qua ảnh — có AI đồng hành cùng bạn.',
      href: isAuthenticated ? '/tu-van-ai' : '/register',
      cta: isAuthenticated ? 'Trải nghiệm ngay →' : 'Đăng ký để bắt đầu →',
    },
  ]

  return (
    <div className="space-y-16">
      <motion.section
        initial="hidden"
        animate="visible"
        variants={fadeInUp}
        className="flex flex-col items-center gap-6 py-8 text-center"
      >
        <span className="flex h-14 w-14 items-center justify-center rounded-full bg-secondary text-primary">
          <Sprout className="h-7 w-7" />
        </span>

        <div className="space-y-3">
          <h1 className="text-display text-text">Kết nối nông dân và người mua trực tiếp</h1>
          <p className="mx-auto max-w-xl text-body text-text-muted">
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
      </motion.section>

      <motion.section
        initial="hidden"
        animate="visible"
        variants={staggerContainer}
        className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4"
      >
        {features.map((feature) => (
          <motion.div key={feature.title} variants={fadeInUp}>
            <Card className="h-full transition-shadow hover:shadow-raised">
              <CardHeader>
                <feature.icon className="h-6 w-6 text-primary" />
                <CardTitle className="pt-2 text-h3">{feature.title}</CardTitle>
                <CardDescription className="text-body-sm">{feature.description}</CardDescription>
              </CardHeader>
              <CardContent>
                <Button variant="link" className="h-auto p-0" asChild>
                  <Link to={feature.href}>{feature.cta}</Link>
                </Button>
              </CardContent>
            </Card>
          </motion.div>
        ))}
      </motion.section>
    </div>
  )
}
