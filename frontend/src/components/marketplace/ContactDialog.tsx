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
import { contactListingSchema, type ContactListingFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

export function ContactDialog({ listingId }: { listingId: number }) {
  const [open, setOpen] = useState(false)
  const contactListing = useContactListing()
  const navigate = useNavigate()

  const form = useForm<ContactListingFormValues>({
    resolver: zodResolver(contactListingSchema),
    defaultValues: { message: '' },
  })

  function onSubmit(values: ContactListingFormValues) {
    contactListing.mutate(
      { id: listingId, message: values.message },
      {
        onSuccess: (interest) => {
          toast.success('Đã gửi liên hệ tới người bán.')
          setOpen(false)
          form.reset()
          navigate(`/marketplace/my-interests/${interest.id}`)
        },
      }
    )
  }

  const errorMessage = contactListing.isError
    ? extractApiErrorMessage(contactListing.error, 'Gửi liên hệ thất bại. Vui lòng thử lại.')
    : null

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <MessageCircle className="h-4 w-4" />
          Liên hệ người bán
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Liên hệ người bán</DialogTitle>
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

            {errorMessage ? <p className="text-sm font-medium text-error">{errorMessage}</p> : null}

            <DialogFooter>
              <DialogClose asChild>
                <Button type="button" variant="outline">
                  Huỷ
                </Button>
              </DialogClose>
              <Button type="submit" disabled={contactListing.isPending}>
                {contactListing.isPending ? 'Đang gửi...' : 'Gửi liên hệ'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
