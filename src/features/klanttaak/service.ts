import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";
import type { KlantTaak } from "./types";
export function useKlantTakenByZaakUrl(zaakUrl: () => string) {
  function getUrl() {
    const zaak = zaakUrl();
    if (!zaak) return "";
    return (
      "/api/klanttaken/api/v2/objects?" +
      new URLSearchParams({
        data_attrs: `zaak__exact__${zaak}`,
      })
    );
  }
  const fetcher = (url: string) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((j) => parsePagination(j, (obj) => obj as KlantTaak));

  return ServiceResult.fromFetcher(getUrl, fetcher);
}
