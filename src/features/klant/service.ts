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
import type { BedrijfSearchParameter } from "./bedrijf/enricher/bedrijf-enricher";
import {
  NietNatuurlijkPersoonIdentifiers,
  usePreferredNietNatuurlijkPersoonIdentifier,
} from "./bedrijf/service/UsePreferredNietNatuurlijkPersoonIdentifier";

type QueryParam = [string, string][];

type FieldParams = {
  email: string;
  telefoonnummer: string;
};

const klantenBaseUrl = "/api/klanten/api/v1/klanten";

export function createKlantQuery<K extends KlantSearchField>(
  args: KlantSearch<K>,
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
klantRootUrl.pathname = klantenBaseUrl;

function getKlantSearchUrl<K extends KlantSearchField>(
  search: KlantSearch<K> | undefined,
  subjectType: KlantType,
  page: number | undefined,
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
  console.log("searchKlanten url", url);
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      p.page.forEach((klant) => {
        console.log("searchKlanten klant found", klant);

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
    fetchKlantById,
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
      }),
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
      page.value,
    );
  return ServiceResult.fromFetcher(getUrl, searchKlanten);
}

export function useKlantByBsn(
  getBsn: () => string | undefined,
): ServiceData<Klant | null> {
  const getUrl = () => getKlantBsnUrl(getBsn());

  console.log("serach klant by BSN ");

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId: () => getSingleBsnSearchId(getBsn()),
  });
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

export async function ensureKlantForBsn(
  {
    bsn,
  }: {
    bsn: string;
  },
  bronorganisatie: string,
) {
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
      bronorganisatie,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
      subjectIdentificatie: { inpBsn: bsn },
      subjectType: KlantType.Persoon,
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

const getUrlVoorGetKlantById = (ding: BedrijfSearchParameter | undefined) => {
  if (!ding) {
    return "";
  }

  if ("vestigingsnummer" in ding) {
    if (!ding.vestigingsnummer) return "";
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjecBedrijf__vestinginsnummer",
      ding.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  } else if ("kvkNummer" in ding) {
    if (!ding.kvkNummer) return "";
    const url = new URL(klantRootUrl);
    // url.searchParams.set("subjectNietnatuurlijkPersoon__nnNip", ding.nnNip);
    url.searchParams.set("kvkNummer", ding.kvkNummer);
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  } else if ("innNnpId" in ding) {
    if (!ding.innNnpId) return "";
    const url = new URL(klantRootUrl);
    // url.searchParams.set("subjectNietnatuurlijkPersoon__nnNip", ding.nnNip);
    url.searchParams.set(
      "subjectNietNatuurlijkPersoon__innNnpId",
      ding.innNnpId,
    );
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

const getUrlVoorGetKlantByVestigingOrRsin = (
  ding: CreateBedrijfKlantIdentifier | undefined,
) => {
  if (!ding) {
    return "";
  }

  if ("vestigingsnummer" in ding) {
    if (!ding.vestigingsnummer) return "";
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjecBedrijf__vestinginsnummer",
      ding.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  } else if ("rsin" in ding) {
    if (!ding.rsin) return "";
    const url = new URL(klantRootUrl);
    // url.searchParams.set("subjectNietnatuurlijkPersoon__nnNip", ding.nnNip);
    url.searchParams.set("subjectNietNatuurlijkPersoon__innNnpId", ding.rsin);
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

const getKlantByNietNatuurlijkpersoonIdentifierUrl = (id: string) => {
  if (!id) return "";
  const url = new URL(klantRootUrl);
  url.searchParams.set("subjectNietNatuurlijkPersoon__innNnpId", id); //todo make arg instelbaar
  url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
  return url.toString();
};

export const useKlantByIdentifier = (
  getId: () => BedrijfSearchParameter | undefined,
) => {
  const getUrl = () => getUrlVoorGetKlantById(getId());

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  console.log("--useKlantByIdentifier", getUrl(), getId());

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId,
  });
};

export type CreateBedrijfKlantIdentifier =
  | {
      vestigingsnummer: string;
    }
  | {
      rsin: string;
    }
  | {
      kvknummer: string;
    };

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForVestigingsnummer(
  {
    bedrijfsnaam,
    identifier,
  }: {
    bedrijfsnaam: string;
    identifier: CreateBedrijfKlantIdentifier;
  },
  bronorganisatie: string,
) {
  const url = getUrlVoorGetKlantByVestigingOrRsin(identifier);
  const uniqueId = url && url + "_single";

  if (!url || !uniqueId) throw new Error();

  const first = await searchSingleKlant(url);

  if (first) {
    mutate(uniqueId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  let subjectType: KlantType | null = null;
  let subjectIdentificatie = {};
  //afhankelijk van het type 'bedrijf' slaan we andere gegevens op
  if ("vestigingsnummer" in identifier) {
    subjectType = KlantType.Bedrijf;
    subjectIdentificatie = { vestigingsNummer: identifier.vestigingsnummer };
  } else {
    //als we niet te maken hebben met een vestiging
    //dan gebruiken we afhankelijk van de mogelijkheden van de gerbuite registers
    //rsin of kvkNummer. We halen de ingestelde voorkeurswaarde identifier op
    //en kijken of dit geven beschikbaar is zodat we hiermee een klant kunnen aanmaken
    const preferredNietNatuurlijkPersoonIdentifier =
      await usePreferredNietNatuurlijkPersoonIdentifier();
    if (
      "rsin" in identifier &&
      preferredNietNatuurlijkPersoonIdentifier.nietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.rsin
    ) {
      subjectType = KlantType.NietNatuurlijkPersoon;
      subjectIdentificatie = { innNnpId: identifier.rsin };
    } else if (
      "kvknummer" in identifier &&
      preferredNietNatuurlijkPersoonIdentifier.nietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.kvkNummer
    ) {
      subjectType = KlantType.NietNatuurlijkPersoon;
      subjectIdentificatie = { innNnpId: identifier.kvknummer };
    }

    console.log("###################");
    console.log(
      preferredNietNatuurlijkPersoonIdentifier.nietNatuurlijkPersoonIdentifier,
    );
    console.log("###################");
  }

  if (!subjectType || !subjectIdentificatie) {
    throw new Error("Kan geen klant aanmaken zonder identificatie");
  }

  const response = await fetchLoggedIn(klantRootUrl, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      bronorganisatie,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
      subjectIdentificatie: subjectIdentificatie,
      subjectType: subjectType, ///
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

export async function ensureKlantForNietNatuurlijkPersoon(
  {
    bedrijfsnaam,
    id,
  }: {
    bedrijfsnaam: string;
    id: string;
  },
  bronorganisatie: string,
) {
  const url = getKlantByNietNatuurlijkpersoonIdentifierUrl(id);
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
      bronorganisatie,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
      subjectIdentificatie: { innNnpId: id }, //todo innNnpId variabel maken
      subjectType: KlantType.NietNatuurlijkPersoon,
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

export function createKlant({
  telefoonnummer = "",
  emailadres = "",
  bronorganisatie = "",
}) {
  if (!bronorganisatie) return Promise.reject();
  if (!telefoonnummer && !emailadres) return Promise.reject();

  return fetchLoggedIn(klantRootUrl, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      bronorganisatie,
      emailadres,
      telefoonnummer,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
      subjectType: KlantType.Persoon,
      subjectIdentificatie: { inpBsn: "" },
    }),
  })
    .then(throwIfNotOk)
    .then(parseJson)
    .then(mapKlant)
    .then((newKlant) => {
      const idUrl = getKlantIdUrl(newKlant.id);
      mutate(idUrl, newKlant);
      return newKlant;
    });
}
