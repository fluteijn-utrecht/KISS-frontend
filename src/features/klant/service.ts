import {
  ServiceResult,
  fetchLoggedIn,
  type Paginated,
  parsePagination,
  throwIfNotOk,
  parseJson,
  type ServiceData,
  enforceOneOrZero,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";

import {
  type UpdateContactgegevensParams,
  type Klant,
  KlantType,
} from "./types";

type QueryParam = [string, string][];

type FieldParams = {
  email: string;
  telefoonnummer: string;
};

export function createKlantQuery<K extends KlantSearchField>(
  args: KlantSearch<K>
): KlantSearch<K> {
  return args;
}

export type KlantSearchField = keyof FieldParams;

type QueryDictionary = {
  [K in KlantSearchField]: (search: FieldParams[K]) => QueryParam;
};

const queryDictionary: QueryDictionary = {
  email: (search) => [["embedded.emails.email[like]", search]],
  telefoonnummer: (search) => [
    ["embedded.telefoonnummers.telefoonnummer[like]", search],
  ],
};

export type KlantSearch<K extends KlantSearchField> = {
  field: K;
  query: FieldParams[K];
};

function getQueryParams<K extends KlantSearchField>(params: KlantSearch<K>) {
  return queryDictionary[params.field](params.query) as ReturnType<
    QueryDictionary[K]
  >;
}

type KlantSearchParameters<K extends KlantSearchField = KlantSearchField> = {
  query: Ref<KlantSearch<K> | undefined>;
  page: Ref<number | undefined>;
};

const klantRootUrl = `${window.gatewayBaseUri}/api/klanten`;

function setExtend(url: URL) {
  url.searchParams.set("extend[]", "all");
}

function getKlantSearchUrl<K extends KlantSearchField>(
  search: KlantSearch<K> | undefined,
  page: number | undefined
) {
  if (!search?.query) return "";

  const url = new URL(klantRootUrl);
  setExtend(url);
  url.searchParams.set("_order[achternaam]", "asc");
  url.searchParams.set("_page", page?.toString() ?? "1");
  url.searchParams.append("subjectType", KlantType.Persoon);

  getQueryParams(search).forEach((tuple) => {
    url.searchParams.set(...tuple);
  });

  return url.toString();
}

function mapKlant(obj: any): Klant {
  const { subjectIdentificatie, emails, telefoonnummers } = obj?.embedded ?? {};
  const { inpBsn, verblijfsadres, geboortedatum, vestigingsNummer } =
    subjectIdentificatie ?? {};
  const { aoaHuisnummer, aoaPostcode } = verblijfsadres ?? {};

  return {
    ...obj,
    _typeOfKlant: "klant",
    emails: emails ?? [],
    telefoonnummers: telefoonnummers ?? [],
    bsn: inpBsn,
    postcode: aoaPostcode,
    huisnummer: aoaHuisnummer,
    geboortedatum: geboortedatum && new Date(geboortedatum),
    vestigingsnummer: vestigingsNummer,
  };
}

function searchKlanten(url: string): Promise<Paginated<Klant>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      p.page.forEach((klant) => {
        const idUrl = getKlantIdUrl(klant.id);
        if (idUrl) {
          mutate(idUrl, klant);
        }
        const bsnUrl = getKlantBsnUrl(klant.bsn);

        if (bsnUrl) {
          mutate(bsnUrl, klant);
        }
      });
      return p;
    });
}

function getKlantIdUrl(id?: string) {
  if (!id) return "";
  const url = new URL(`${klantRootUrl}/${id}`);
  setExtend(url);
  return url.toString();
}

function getKlantBsnUrl(bsn?: string) {
  if (!bsn) return "";
  const url = new URL(klantRootUrl);
  setExtend(url);
  url.searchParams.set("embedded.subjectIdentificatie.inpBsn", bsn);
  return url.toString();
}

const searchSingleKlant = (url: string) =>
  searchKlanten(url).then(enforceOneOrZero);

const getSingleBsnSearchId = (bsn: string | undefined) => {
  const url = getKlantBsnUrl(bsn);
  if (!url) return url;
  return url + "_single";
};

function fetchKlantById(url: string) {
  return fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapKlant);
}

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => getKlantIdUrl(id.value),
    fetchKlantById
  );
}

function updateContactgegevens({
  id,
  telefoonnummers,
  emails,
}: UpdateContactgegevensParams): Promise<UpdateContactgegevensParams> {
  const url = klantRootUrl + "/" + id;
  return fetchLoggedIn(url + "?fields[]=klantnummer&fields[]=bronorganisatie")
    .then(throwIfNotOk)
    .then(parseJson)
    .then((klant) =>
      fetchLoggedIn(url, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          ...klant,
          embedded: {
            telefoonnummers,
            emails,
          },
        }),
      })
    )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ embedded }) => ({
      id,
      telefoonnummers: embedded?.telefoonnummers ?? [],
      emails: embedded?.emails ?? [],
    }));
}

export function useSearchKlanten<K extends KlantSearchField>({
  query,
  page,
}: KlantSearchParameters<K>) {
  const getUrl = () => getKlantSearchUrl(query.value, page.value);
  return ServiceResult.fromFetcher(getUrl, searchKlanten);
}

export function useKlantByBsn(
  getBsn: () => string | undefined
): ServiceData<Klant | null> {
  const getUrl = () => getKlantBsnUrl(getBsn());

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId: () => getSingleBsnSearchId(getBsn()),
  });
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

export async function ensureKlantForBsn({
  bsn,
  voornaam,
  voorvoegselAchternaam,
  achternaam,
}: {
  bsn: string;
  voornaam?: string;
  voorvoegselAchternaam?: string;
  achternaam?: string;
}) {
  const bsnUrl = getKlantBsnUrl(bsn);
  const singleBsnId = getSingleBsnSearchId(bsn);

  if (!bsnUrl || !singleBsnId) throw new Error();

  const first = await searchSingleKlant(bsnUrl);

  if (first) {
    mutate(singleBsnId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  const response = await fetchLoggedIn(klantRootUrl, {
    method: "POST",
    body: JSON.stringify({
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectIdentificatie: { inpBsn: bsn },
      subjectType: KlantType.Persoon,
      voornaam,
      voorvoegselAchternaam,
      achternaam,
    }),
  });

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(singleBsnId, newKlant);

  return newKlant;
}

const getKlantByVestigingsnummerUrl = (vestigingsnummer: string) => {
  if (!vestigingsnummer) return "";
  const url = new URL(klantRootUrl);
  url.searchParams.set("extend[]", "all");
  url.searchParams.set(
    "embedded.subjectIdentificatie.vestigingsNummer",
    vestigingsnummer
  );
  url.searchParams.set("subjectType", KlantType.Bedrijf);
  return url.toString();
};

export const useKlantByVestigingsnummer = (
  getVestigingsnummer: () => string | undefined
) => {
  const getUrl = () =>
    getKlantByVestigingsnummerUrl(getVestigingsnummer() ?? "");

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId,
  });
};

export async function ensureKlantForVestigingsnummer({
  bedrijfsnaam,
  vestigingsnummer,
}: {
  vestigingsnummer: string;
  bedrijfsnaam: string;
}) {
  const url = getKlantByVestigingsnummerUrl(vestigingsnummer);
  const uniqueId = url && url + "_single";

  if (!url || !uniqueId) throw new Error();

  const first = await searchSingleKlant(url);

  if (first) {
    mutate(uniqueId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  const response = await fetchLoggedIn(klantRootUrl, {
    method: "POST",
    body: JSON.stringify({
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectIdentificatie: { vestigingsNummer: vestigingsnummer },
      subjectType: KlantType.Bedrijf,
      bedrijfsnaam,
    }),
  });

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(uniqueId, newKlant);

  return newKlant;
}
