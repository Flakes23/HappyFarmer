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
          // Màu theo loại thông báo — dùng token thương hiệu (success/destructive/accent/primary)
          // nên tự đổi theo light/dark mode. `!` để thắng class nền mặc định của `toast` ở trên.
          success:
            "!border-success !bg-success !text-success-foreground [&_[data-description]]:!text-success-foreground/90",
          error:
            "!border-destructive !bg-destructive !text-destructive-foreground [&_[data-description]]:!text-destructive-foreground/90",
          warning:
            "!border-accent !bg-accent !text-accent-foreground [&_[data-description]]:!text-accent-foreground/90",
          info:
            "!border-primary !bg-primary !text-primary-foreground [&_[data-description]]:!text-primary-foreground/90",
        },
      }}
      {...props}
    />
  )
}

export { Toaster }
