import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  throwIfNotOk,
} from "@/services";
import type { ZaakDetails } from "./types";
import { computed } from "vue";

export const useZaaksysteemDeeplinkConfig = () =>
  ServiceResult.fromFetcher("/api/zaaksysteem/deeplinkconfig", (u) =>
    fetchLoggedIn(u)
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
  );

export function useZaaksysteemDeeplink(getZaak: () => ZaakDetails | undefined) {
  const config = useZaaksysteemDeeplinkConfig();
  return computed(() => {
    if (!config.success || !config.data) return null;
    const zaak = getZaak();
    const property = (zaak as any)?.[config.data.idProperty];
    if (!property) return null;
    return config.data.baseUrl + property;
  });
}
