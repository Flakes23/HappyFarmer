import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Pencil } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
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
import { useUpdateListing } from '@/hooks/mutations/useUpdateListing'
import { updateListingSchema, type UpdateListingFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'
import type { ListingResponse } from '@/api/types'

export function EditListingDialog({ listing }: { listing: ListingResponse }) {
  const [open, setOpen] = useState(false)
  const updateListing = useUpdateListing()

  const form = useForm<UpdateListingFormValues>({
    resolver: zodResolver(updateListingSchema),
    defaultValues: {
      quantity: listing.quantity,
      pricePerUnit: listing.pricePerUnit,
      description: listing.description ?? '',
    },
  })

  function onSubmit(values: UpdateListingFormValues) {
    const parsed = updateListingSchema.parse(values)
    updateListing.mutate(
      { id: listing.id, body: parsed },
      {
        onSuccess: () => {
          toast.success('Cập nhật tin đăng thành công.')
          setOpen(false)
        },
      }
    )
  }

  const errorMessage = updateListing.isError
    ? extractApiErrorMessage(updateListing.error, 'Cập nhật thất bại. Vui lòng thử lại.')
    : null

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          <Pencil className="h-4 w-4" />
          Sửa
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Sửa tin đăng</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="quantity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Số lượng ({listing.unit})</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.01" {...field} value={field.value as number | string} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="pricePerUnit"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Giá (VNĐ) / đơn vị</FormLabel>
                  <FormControl>
                    <Input type="number" step="1000" {...field} value={field.value as number | string} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Mô tả</FormLabel>
                  <FormControl>
                    <Textarea {...field} />
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
              <Button type="submit" disabled={updateListing.isPending}>
                {updateListing.isPending ? 'Đang lưu...' : 'Lưu thay đổi'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
