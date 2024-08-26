import { ServiceResult } from "@/services";
import type { Ref } from "vue";
import { fetchKlantById } from "@/services/klanten";

export const useKlantById = (id: Ref<string>) =>
  ServiceResult.fromFetcher(() => id.value || "", fetchKlantById);
