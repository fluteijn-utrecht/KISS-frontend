import type { PaginatedResult } from "./gateway";

export interface Paginated<T> {
  pageSize?: number;
  pageNumber?: number | string;
  totalPages?: number;
  totalRecords?: number;
  page: T[];
}

export function defaultPagination<T>(page: T[]): Paginated<T> {
  return {
    page,
    pageNumber: 1,
    totalPages: 1,
    totalRecords: page.length,
    pageSize: page.length,
  };
}

export function enforceOneOrZero<T>(
  paginated: Paginated<NonNullable<T>> | PaginatedResult<NonNullable<T>>
): T | null {
  if (paginated.page.length === 0) return null;
  if (paginated.page.length === 1) return paginated.page[0];
  throw new Error(
    "expected a single result, instead found " + paginated.page.length
  );
}
