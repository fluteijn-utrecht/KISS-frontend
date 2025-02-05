import { ServiceResult } from "@/services";
import type { Ref } from "vue";
import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";
import { getRegisterDetails } from "@/features/shared/systeemdetails";

export const useKlantById = (id: Ref<string>) => {
  const getApiSpecifiekUrl = () => id.value || "";

  const fetchKlant = async (url: string) => {
    if (!url) return null;

    const { useKlantInteractiesApi, systeemId } = await getRegisterDetails();

    return useKlantInteractiesApi
      ? fetchKlantByIdOk2(systeemId, url)
      : fetchKlantByIdOk1(url);
  };

  return ServiceResult.fromFetcher(getApiSpecifiekUrl, fetchKlant);
};
