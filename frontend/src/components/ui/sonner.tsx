import { useTheme } from "next-themes"
import { Toaster as Sonner } from "sonner"

type ToasterProps = React.ComponentProps<typeof Sonner>

const Toaster = ({ ...props }: ToasterProps) => {
  const { theme = "system" } = useTheme()

  return (
    <Sonner
      theme={theme as ToasterProps["theme"]}
      className="toaster group"
      toastOptions={{
        classNames: {
          toast:
            "group toast group-[.toaster]:bg-background group-[.toaster]:text-foreground group-[.toaster]:border-border group-[.toaster]:shadow-lg",
          description: "group-[.toast]:text-muted-foreground",
          actionButton:
            "group-[.toast]:bg-primary group-[.toast]:text-primary-foreground",
          cancelButton:
            "group-[.toast]:bg-muted group-[.toast]:text-muted-foreground",
          // Màu theo loại thông báo — nền giữ nguyên theo giao diện (sáng/tối), chỉ viền trái dày,
          // viền, icon và tiêu đề đổi màu theo token thương hiệu để dịu mắt, dễ đọc ở cả 2 chế độ.
          success: "!border-success/40 !border-l-4 !border-l-success !text-success",
          error: "!border-destructive/40 !border-l-4 !border-l-destructive !text-destructive",
          warning: "!border-accent/40 !border-l-4 !border-l-accent !text-accent",
          info: "!border-primary/40 !border-l-4 !border-l-primary !text-primary",
        },
      }}
      {...props}
    />
  )
}

export { Toaster }
