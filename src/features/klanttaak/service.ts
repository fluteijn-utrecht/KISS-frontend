import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";
import type { KlantTaak, ObjectWrapper } from "./types";
import { formatIsoDate } from "@/helpers/date";

const fetcher = (url: string) =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, (obj) => obj as ObjectWrapper<KlantTaak>));

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

function createTaak(taak: KlantTaak) {
  const now = new Date();
  const startAt = formatIsoDate(now);
  return fetchLoggedIn("/api/klanttaken/api/v2/objects", {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      record: {
        typeVersion: 1,
        startAt,
        data: taak,
      },
    }),
  }).then(throwIfNotOk);
}

export const useCreateTaak = () => ServiceResult.fromSubmitter(createTaak);
