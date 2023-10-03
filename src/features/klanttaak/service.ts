import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";
import type { KlantTaak } from "./types";

const fetcher = (url: string) =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, (obj) => obj as KlantTaak));

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

  return ServiceResult.fromFetcher(getUrl, fetcher);
}

export function useKlantTakenByBsn(getBsn: () => string) {
  function getUrl() {
    const bsn = getBsn();
    if (!bsn) return "";
    const params = new URLSearchParams();
    params.append("data_attrs", "identificatie__type__exact__bsn");
    params.append("data_attrs", `identificatie__value__exact__${bsn}`);

    return "/api/klanttaken/api/v2/objects?" + params;
  }

  return ServiceResult.fromFetcher(getUrl, fetcher);
}
