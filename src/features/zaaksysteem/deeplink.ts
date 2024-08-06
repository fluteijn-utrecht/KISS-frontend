import { ServiceResult, parseJson, throwIfNotOk } from "@/services";
import type { ZaakDetails } from "./types";
import { computed } from "vue";
import { fetchWithZaaksysteemId } from "./service";

const useZaaksysteemDeeplinkConfig = (
  zaaksysteemId: () => string | undefined,
) => {
  const url = "/api/zaaksysteem/deeplinkconfig";
  const getCacheKey = () => {
    const id = zaaksysteemId() || "";
    return id && url + id;
  };
  return ServiceResult.fromFetcher(
    url,
    (u) =>
      fetchWithZaaksysteemId(zaaksysteemId(), u)
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

export function useZaaksysteemDeeplink(
  getZaak: () => ZaakDetails | undefined,
  getZaaksysteemId: () => string | undefined,
) {
  const config = useZaaksysteemDeeplinkConfig(getZaaksysteemId);
  return computed(() => {
    if (!config.success || !config.data) return null;
    const property = (getZaak() as any)?.[config.data.idProperty];
    if (!property) return null;
    return config.data.baseUrl + property;
  });
}
