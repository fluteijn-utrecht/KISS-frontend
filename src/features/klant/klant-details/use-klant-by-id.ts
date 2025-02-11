import type { Ref } from "vue";
import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";
import { ServiceResult } from "@/services";

export const useKlantById = (
  id: Ref<string>,
  defaultSysteemId: Ref<string | null>,
  gebruikKlantInteractiesApi: Ref<boolean | null>,
) => {
  const getApiSpecifickUrl = () => {
    if (gebruikKlantInteractiesApi.value === null) {
      return "";
    }

    return id.value;
  };

  const getDefaultSysteemId = () => {
    if (defaultSysteemId.value === null) {
      return "";
    }

    return defaultSysteemId.value;
  };

  const fetchKlant = (
    url: string,
    gebruikKlantinteractiesApi: Ref<boolean | null>,
  ) => {
    if (gebruikKlantinteractiesApi.value) {
      return fetchKlantByIdOk2(getDefaultSysteemId(), url);
    } else {
      return fetchKlantByIdOk1(getDefaultSysteemId(), url);
    }
  };

  return ServiceResult.fromFetcher(getApiSpecifickUrl, (u: string) =>
    fetchKlant(u, gebruikKlantInteractiesApi),
  );
};
