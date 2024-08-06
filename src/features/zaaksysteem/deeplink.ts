import { ServiceResult, parseJson, throwIfNotOk } from "@/services";
import type { ZaakDetails } from "./types";
import { computed, type Ref } from "vue";
import { fetchWithZaaksysteemId } from "./service";

const useZaaksysteemDeeplinkConfig = (
  zaaksysteemId: Ref<string | undefined>,
) => {
  const url = "/api/zaaksysteem/deeplinkconfig";
  const getCacheKey = () => {
    const id = zaaksysteemId.value || "";
    return id && url + id;
  };
  return ServiceResult.fromFetcher(
    url,
    (u) =>
      fetchWithZaaksysteemId(zaaksysteemId.value, u)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((r) =>
          typeof r?.baseUrl === "string" &&
          typeof r?.idProperty === "string" &&
          r.baseUrl &&
          r.idProperty
            ? (r as {
                baseUrl: string;
                idProperty: string;
              })
            : null,
        ),
    {
      getUniqueId: getCacheKey,
    },
  );
};

export function useZaaksysteemDeeplink(getZaak: () => ZaakDetails | undefined) {
  const zaak = computed(getZaak);
  const zaaksysteemId = computed(() => zaak.value?.zaaksysteemId);
  const config = useZaaksysteemDeeplinkConfig(zaaksysteemId);
  return computed(() => {
    if (!config.success || !config.data) return null;
    const property = (zaak.value as any)?.[config.data.idProperty];
    if (!property) return null;
    return config.data.baseUrl + property;
  });
}
