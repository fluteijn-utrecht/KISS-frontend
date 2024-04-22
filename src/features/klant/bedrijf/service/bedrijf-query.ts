import type { BedrijfQuery, SearchCategories } from "../types";

export const bedrijfQuery = <K extends SearchCategories>(
  query: BedrijfQuery<K>,
) => query;
