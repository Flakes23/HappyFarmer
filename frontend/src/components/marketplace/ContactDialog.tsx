import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { MessageCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { useContactListing } from '@/hooks/mutations/useContactListing'
import { useContactBuyRequest } from '@/hooks/mutations/useContactBuyRequest'
import { contactListingSchema, type ContactListingFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

export function ContactDialog({ id, kind }: { id: number; kind: 'listing' | 'buyRequest' }) {
  const [open, setOpen] = useState(false)
  const contactListing = useContactListing()
  const contactBuyRequest = useContactBuyRequest()
  const mutation = kind === 'listing' ? contactListing : contactBuyRequest
  const label = kind === 'listing' ? 'Liên hệ người bán' : 'Liên hệ người mua'
  const navigate = useNavigate()

  const form = useForm<ContactListingFormValues>({
    resolver: zodResolver(contactListingSchema),
    defaultValues: { message: '' },
  })

  function onSubmit(values: ContactListingFormValues) {
    mutation.mutate(
      { id, message: values.message },
      {
        onSuccess: (interest) => {
          toast.success(`Đã gửi ${label.toLowerCase()}.`)
          setOpen(false)
          form.reset()
          navigate(`/marketplace/my-interests/${interest.id}`)
        },
      }
    )
  }

  const errorMessage = mutation.isError
    ? extractApiErrorMessage(mutation.error, 'Gửi liên hệ thất bại. Vui lòng thử lại.')
    : null

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <MessageCircle className="h-4 w-4" />
          {label}
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{label}</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="message"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Lời nhắn</FormLabel>
                  <FormControl>
                    <Textarea placeholder="Tôi muốn mua..." {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {errorMessage ? <p className="text-body-sm font-medium text-error">{errorMessage}</p> : null}

            <DialogFooter>
              <DialogClose asChild>
                <Button type="button" variant="outline">
                  Huỷ
                </Button>
              </DialogClose>
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending ? 'Đang gửi...' : 'Gửi liên hệ'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
