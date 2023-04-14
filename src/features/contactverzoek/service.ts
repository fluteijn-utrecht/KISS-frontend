import {
  fetchLoggedIn,
  getFormattedUtcDate,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
} from "@/services";
import type { NieuweKlant } from "@/stores/contactmoment";
// creating a klant will be done differently in the future. for now, jus reuse the type from the klant feature
import { KlantType } from "../klant/types";

export interface Contactverzoek {
  bronorganisatie: string; //verplicht in de api
  todo: {
    name: "contactverzoek";
    description: string;
    attendees: string[];
  };
  primaireVraagWeergave?: string;
  afwijkendOnderwerp?: string;
}

export function saveContactverzoek(data: Contactverzoek) {
  const url = window.gatewayBaseUri + "/api/contactmomenten";
  const registratiedatum = getFormattedUtcDate();

  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      ...data,
      registratiedatum,
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json() as Promise<{ id: string; url: string }>);
}

export function createKlant(klant: NieuweKlant) {
  const url = `${window.gatewayBaseUri}/api/klanten`;
  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      ...klant,
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectType: KlantType.Persoon,
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());
}

interface Afdeling {
  id: string;
  name: string;
}

export function useAfdelingen() {
  const url = `${window.gatewayBaseUri}/api/ref/afdelingsnamen`;

  const mapOrganisatie = (x: unknown): Afdeling => x as any;

  const fetcher = (url: string, page = 1, limit = 100): Promise<Afdeling[]> =>
    fetchLoggedIn(`${url}?_limit=${limit}&_page=${page}`)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((json) => parsePagination(json, mapOrganisatie))
      .then(async (current) => {
        if (current.totalPages <= current.pageNumber) return current.page;
        const nextPage = await fetcher(url, page + 1);
        return [...current.page, ...nextPage];
      })
      .then((all) => all.sort((a, b) => a.name.localeCompare(b.name)));

  return ServiceResult.fromFetcher(url, fetcher);
}
