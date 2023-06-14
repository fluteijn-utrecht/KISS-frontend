import {
  ServiceResult,
  fetchLoggedIn,
  parsePagination,
  throwIfNotOk,
  parseJson,
  type ServiceData,
  enforceOneOrZero,
  type PaginatedResult,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";

import {
  type UpdateContactgegevensParams,
  type Klant,
  KlantType,
} from "./types";
import { nanoid } from "nanoid";

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
  email: (search) => [["emailadres", search]],
  telefoonnummer: (search) => [["telefoonnummer", search]],
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
  subjectType?: KlantType;
};

const klantRootUrl = new URL(document.location.href);
klantRootUrl.pathname = "/api/klanten/api/v1/klanten";

function getKlantSearchUrl<K extends KlantSearchField>(
  search: KlantSearch<K> | undefined,
  subjectType: KlantType,
  page: number | undefined
) {
  if (!search?.query) return "";

  const url = new URL(klantRootUrl);
  url.searchParams.set("page", page?.toString() ?? "1");
  url.searchParams.append("subjectType", subjectType);

  getQueryParams(search).forEach((tuple) => {
    url.searchParams.set(...tuple);
  });

  return url.toString();
}

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

function searchKlanten(url: string): Promise<PaginatedResult<Klant>> {
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
  return url.toString();
}

function getKlantBsnUrl(bsn?: string) {
  if (!bsn) return "";
  const url = new URL(klantRootUrl);
  url.searchParams.set("subjectNatuurlijkPersoon__inpBsn", bsn);
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

const getValidIdentificatie = ({ subjectType, subjectIdentificatie }: any) => {
  if (subjectType === KlantType.Persoon)
    return subjectIdentificatie || { inpBsn: "" };

  const { handelsnaam, ...rest } = subjectIdentificatie ?? {};
  if (Array.isArray(handelsnaam) && handelsnaam.length)
    return subjectIdentificatie;
  return rest;
};

function updateContactgegevens({
  id,
  telefoonnummer,
  emailadres,
}: UpdateContactgegevensParams): Promise<UpdateContactgegevensParams> {
  const endpoint = klantRootUrl + "/" + id;
  return fetchLoggedIn(endpoint)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((klant) =>
      fetchLoggedIn(endpoint, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          ...klant,
          subjectIdentificatie: getValidIdentificatie(klant),
          telefoonnummer,
          emailadres,
        }),
      })
    )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ telefoonnummer, emailadres }) => ({
      id,
      telefoonnummer,
      emailadres,
    }));
}

export function useSearchKlanten<K extends KlantSearchField>({
  query,
  page,
  subjectType,
}: KlantSearchParameters<K>) {
  const getUrl = () =>
    getKlantSearchUrl(
      query.value,
      subjectType ?? KlantType.Persoon,
      page.value
    );
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
    headers: { "content-type": "application/json" },
    body: JSON.stringify({
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
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
  url.searchParams.set("subjectVestiging__vestigingsNummer", vestigingsnummer);
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
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
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
