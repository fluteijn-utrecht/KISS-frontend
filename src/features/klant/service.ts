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
import type { BedrijfIdentifier } from "./bedrijf/types";
import { klantinteractieClient } from "./klantinteractie-client";
import {
  SoortPartijEnum,
  type ExpandPartij,
  type Partij,
  type PartijIdentificator,
} from "@/../generated/klantinteracties";

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
  const { inpBsn, vestigingsNummer, innNnpId } = subjectIdentificatie ?? {};
  const urlSplit: string[] = url?.split("/") ?? [];

  return {
    ...obj,
    id: urlSplit[urlSplit.length - 1],
    _typeOfKlant: "klant",
    bsn: inpBsn,
    vestigingsnummer: vestigingsNummer,
    url: url,
    nietNatuurlijkPersoonIdentifier: innNnpId,
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

function mapPartijToKlant(
  partij: ExpandPartij,
  identificatoren: PartijIdentificator[],
): Klant {
  const getAdres = (type: string) =>
    partij._expand?.digitaleAdressen?.find(
      (x) => x.adres && x.soortDigitaalAdres === type,
    )?.adres;

  const getId = (type: string) =>
    identificatoren.find(
      (x) =>
        x.partijIdentificator?.objectId &&
        x.partijIdentificator?.codeSoortObjectId == type,
    )?.partijIdentificator?.objectId;

  return {
    _typeOfKlant: "klant",
    klantnummer: partij.nummer || "",
    id: partij.uuid,
    url: partij.url,
    telefoonnummer: getAdres("telefoonnummer"),
    emailadres: getAdres("emailadres"),
    bsn: getId("Burgerservicenummer"),
    // bedrijfsnaam?: string;
    vestigingsnummer: getId("Vestigingsnummer"),
    nietNatuurlijkPersoonIdentifier: getId("nnnpId"),
  };
}

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => id.value,
    (id) =>
      klantinteractieClient.partijen
        .partijenRetrieve(id, [
          "betrokkenen",
          "betrokkenen.hadKlantcontact",
          "categorieRelaties",
          "digitaleAdressen",
        ])
        .then(async (partij) => {
          const promises = partij.partijIdentificatoren.map(({ uuid }) =>
            klantinteractieClient.partijIdentificatoren.partijIdentificatorenRetrieve(
              uuid,
            ),
          );
          const identificatoren = await Promise.all(promises);
          return mapPartijToKlant(partij, identificatoren);
        }),
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

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId: () => getSingleBsnSearchId(getBsn()),
  });
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

const getSingleKlantByBsn = (bsn: string) =>
  klantinteractieClient.partijen
    .partijenList(
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      undefined,
      bsn,
      "persoon",
    )
    .then(
      ({ results, ...rest }) =>
        ({
          page: results,
          ...rest,
        }) as PaginatedResult<Partij>,
    )
    .then(enforceOneOrZero)
    .then(async (partij) => {
      if (!partij) return partij;
      const promises = partij.partijIdentificatoren.map(({ uuid }) =>
        klantinteractieClient.partijIdentificatoren.partijIdentificatorenRetrieve(
          uuid,
        ),
      );
      const identificatoren = await Promise.all(promises);
      return mapPartijToKlant(partij, identificatoren);
    });

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

  const first = await getSingleKlantByBsn(bsn);

  if (first) {
    mutate(singleBsnId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  const partij = await klantinteractieClient.partijen.partijenCreate({
    url: "",
    uuid: "",
    digitaleAdressen: [],
    betrokkenen: [],
    categorieRelaties: [],
    voorkeursDigitaalAdres: null,
    vertegenwoordigden: [],
    rekeningnummers: [],
    partijIdentificatoren: [],
    voorkeursRekeningnummer: null,
    soortPartij: SoortPartijEnum.PERSOON,
    indicatieGeheimhouding: false,
    indicatieActief: true,
  });

  const identificator =
    await klantinteractieClient.partijIdentificatoren.partijIdentificatorenCreate(
      {
        uuid: "",
        url: "",
        identificeerdePartij: {
          url: partij.url,
          uuid: partij.uuid,
        },
        partijIdentificator: {
          objectId: bsn,
          codeRegister: "BRP",
          codeSoortObjectId: "Burgerservicenummer",
          codeObjecttype: "INGESCHREVEN NATUURLIJK PERSOON",
        },
      },
    );

  const newKlant = mapPartijToKlant(partij, [identificator]);

  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(singleBsnId, newKlant);

  return newKlant;
}

const getUrlVoorGetKlantById = (
  bedrijfSearchParameter: BedrijfIdentifier | undefined,
) => {
  if (!bedrijfSearchParameter) {
    return "";
  }

  if (
    "vestigingsnummer" in bedrijfSearchParameter &&
    bedrijfSearchParameter.vestigingsnummer
  ) {
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjectVestiging__vestigingsNummer",
      bedrijfSearchParameter.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  }

  if (
    "nietNatuurlijkPersoonIdentifier" in bedrijfSearchParameter &&
    bedrijfSearchParameter.nietNatuurlijkPersoonIdentifier
  ) {
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjectNietNatuurlijkPersoon__innNnpId",
      bedrijfSearchParameter.nietNatuurlijkPersoonIdentifier,
    );
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

const getKlantByNietNatuurlijkpersoonIdentifierUrl = (id: string) => {
  if (!id) return "";
  const url = new URL(klantRootUrl);
  url.searchParams.set("subjectNietNatuurlijkPersoon__innNnpId", id);
  url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
  return url.toString();
};

export const useKlantByIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
) => {
  const getUrl = () => getUrlVoorGetKlantById(getId());

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId,
  });
};

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForBedrijfIdentifier(
  {
    bedrijfsnaam,
    identifier,
  }: {
    bedrijfsnaam: string;
    identifier: BedrijfIdentifier;
  },
  bronorganisatie: string,
) {
  const url = getUrlVoorGetKlantById(identifier);
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
  if ("vestigingsnummer" in identifier && identifier.vestigingsnummer) {
    subjectType = KlantType.Bedrijf;
    subjectIdentificatie = { vestigingsNummer: identifier.vestigingsnummer };
  } else if (
    "nietNatuurlijkPersoonIdentifier" in identifier &&
    identifier.nietNatuurlijkPersoonIdentifier
  ) {
    //als we niet te maken hebben met een vestiging
    //dan gebruiken we afhankelijk van de mogelijkheden van de gerbuite registers
    subjectType = KlantType.NietNatuurlijkPersoon;
    subjectIdentificatie = {
      innNnpId: identifier.nietNatuurlijkPersoonIdentifier,
    };
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
      klantnummer: nanoid(8),
      subjectIdentificatie: { innNnpId: id },
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
