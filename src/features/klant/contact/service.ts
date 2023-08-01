import {
  ServiceResult,
  fetchLoggedIn,
  parsePagination,
  throwIfNotOk,
  parseJson,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";
import { KlantType, type Klant } from "../types";

const klantenBaseUrl = "/api/klanten/api/v1/klanten";

type KlantSearchParameters = {
  email?: string;
  phone?: string;
};

const klantRootUrl = new URL(document.location.href);
klantRootUrl.pathname = klantenBaseUrl;

function getKlantSearchUrl({ email = "", phone = "" }: KlantSearchParameters) {
  if (!email && !phone) return "";

  const url = new URL(klantRootUrl);

  url.searchParams.set("subjectType", KlantType.Persoon);

  if (email) {
    url.searchParams.set("emailadres", email);
  }

  if (phone) {
    url.searchParams.set("telefoonnummer", phone);
  }

  return url.toString();
}

// TODO: kijken of een gemeenschappelijke interface nog nodig is als het zoeken op contact/persoon/bedrijf uitgewerkt is
function mapKlant(obj: any): Klant {
  const { subjectIdentificatie, url } = obj ?? {};
  const { inpBsn, vestigingsNummer } = subjectIdentificatie ?? {};
  const urlSplit: string[] = url?.split("/") ?? [];

  return {
    ...obj,
    id: urlSplit[urlSplit.length - 1],
    _typeOfKlant: "klant",
    bsn: inpBsn,
    vestigingsnummer: vestigingsNummer,
    url: url,
  };
}

function searchKlantenRecursive(urlStr: string, page = 1): Promise<Klant[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      // remove klanten with bsn
      p.page = p.page.filter((x) => !x.bsn);
      return p;
    })
    .then((p) => {
      p.page.forEach((klant) => {
        const idUrl = getKlantIdUrl(klant.id);
        if (idUrl) {
          mutate(idUrl, klant);
        }
      });
      return p;
    })
    .then((p) => {
      if (!p.next) return p.page;
      // because we filter out results manually, we break pagination.
      // to work around this, we retrieve all pages and return them as a flat list.
      return searchKlantenRecursive(urlStr, page + 1).then((next) => [
        ...p.page,
        ...next,
      ]);
    });
}

function compareNonEmpty(
  a: string | null | undefined,
  b: string | null | undefined
) {
  if (!a && !b) return 0;
  if (!b) return -1;
  if (!a) return 1;
  return a.localeCompare(b);
}

function sortKlant(a: Klant, b: Klant) {
  const compareEmail = compareNonEmpty(a.emailadres, b.emailadres);
  if (compareEmail !== 0) return compareEmail;
  return compareNonEmpty(a.telefoonnummer, b.telefoonnummer);
}

function searchKlanten(url: string) {
  return searchKlantenRecursive(url).then((r) => r.sort(sortKlant));
}

function getKlantIdUrl(id?: string) {
  if (!id) return "";
  const url = new URL(`${klantRootUrl}/${id}`);
  return url.toString();
}

function fetchKlantById(url: string) {
  return fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapKlant);
}

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => getKlantIdUrl(id.value),
    fetchKlantById
  );
}

export function useSearchKlanten(params: Ref<KlantSearchParameters>) {
  const getUrl = () => getKlantSearchUrl(params.value);
  return ServiceResult.fromFetcher(getUrl, searchKlanten);
}
