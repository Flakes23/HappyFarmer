import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"
import { format } from "date-fns"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getInitial(name: string) {
  return name.trim().charAt(0).toUpperCase() || '?'
}

/** Parse chuỗi "yyyy-MM-dd" (DateOnly) thành Date theo giờ local — tránh new Date(iso) parse theo UTC gây lệch ngày. */
export function parseIsoDate(value: string): Date {
  const [y, m, d] = value.split('-').map(Number)
  return new Date(y, m - 1, d)
}

/** Định dạng d/M/yyyy (không đệm số 0) cho các trường DateOnly ("yyyy-MM-dd") của backend. */
export function formatViDate(value: string): string {
  return format(parseIsoDate(value), 'd/M/yyyy')
}
