import {
  ServiceResult,
  fetchLoggedIn,
  parsePagination,
  throwIfNotOk,
  parseJson,
  enforceOneOrZero,
  type PaginatedResult,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";

import { type Klant } from "./types";
import type { BedrijfIdentifier } from "./bedrijf/types";

const klantinteractiesBaseUrl = "/api/klantinteracties/api/v1";

type IdentificatorType = {
  codeRegister: string;
  codeSoortObjectId: string;
  codeObjecttype: string;
};

type Partij = {
  nummer?: string;
  uuid: string;
  url: string;
  partijIdentificatoren: { uuid: string }[];
  _expand?: {
    digitaleAdressen?: { adres?: string; soortDigitaalAdres?: string }[];
  };
};

// TODO in toekomstige story: waardes overleggen met Maykin en INFO
const identificatorTypes: Record<string, IdentificatorType> = {
  persoon: {
    codeRegister: "brp",
    codeSoortObjectId: "bsn",
    codeObjecttype: "inp",
  },
  vestiging: {
    codeRegister: "hr",
    codeSoortObjectId: "vtn",
    codeObjecttype: "vst",
  },
  nietNatuurlijkPersoonRsin: {
    codeRegister: "hr",
    codeSoortObjectId: "rsin",
    codeObjecttype: "innp",
  },
  nietNatuurlijkPersoonKvkNummer: {
    codeRegister: "hr",
    codeSoortObjectId: "kvkNummer",
    codeObjecttype: "innp",
  },
};

type Contactnaam = {
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
};

type OrganisatieIdentificatie = {
  naam: string;
};

type PersoonIdentificatie = {
  contactnaam: Contactnaam;
};

export enum PartijTypes {
  persoon = "persoon",
  organisatie = "organisatie",
  contactpersoon = "contactpersoon",
}

type CreatePersoonParameters = {
  soortPartij: PartijTypes.persoon | PartijTypes.contactpersoon;
  partijIdentificatie: PersoonIdentificatie;
};

type CreateOrganisatieParameters = {
  soortPartij: PartijTypes.organisatie;
  partijIdentificatie: OrganisatieIdentificatie;
};

type CreateKlantParameters =
  | CreatePersoonParameters
  | CreateOrganisatieParameters;

const digitaalAdresTypes = {
  email: "email",
  telefoonnummer: "telnr",
} as const;

export type KlantSearchField = "email" | "telefoonnummer";

export type KlantSearch = {
  field: KlantSearchField;
  query: string;
};

type KlantSearchParameters = {
  query: Ref<KlantSearch | undefined>;
  page: Ref<number | undefined>;
  subjectType: PartijTypes;
};

function fetchKlantenOverview(url: string): Promise<PaginatedResult<Klant>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (x) => mapPartijToKlant(x as Partij)));
}

const getKlantIdUrl = (uuid: string) =>
  uuid &&
  `${klantinteractiesBaseUrl}/partijen/${uuid}?${new URLSearchParams({ expand: "digitaleAdressen" })}`;

const searchSingleKlant = (url: string) =>
  fetchKlantenOverview(url).then(enforceOneOrZero);

const fetchSingleKlant = (url: string) =>
  fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapPartijToKlant);

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => getKlantIdUrl(id.value),
    fetchSingleKlant,
  );
}

export function useSearchKlanten({
  query,
  page,
  subjectType,
}: KlantSearchParameters) {
  function getUrl() {
    if (!query.value?.query) return "";

    const searchParams = new URLSearchParams();
    searchParams.set("page", page?.toString() ?? "1");
    searchParams.append("verstrektDoorPartij__soortPartij", subjectType);
    searchParams.append("adres__icontains", query.value.query);

    searchParams.append(
      "soortDigitaalAdres",
      query.value.field === "telefoonnummer"
        ? digitaalAdresTypes.telefoonnummer
        : digitaalAdresTypes.email,
    );

    return klantinteractiesBaseUrl + "/digitaleadressen?" + searchParams;
  }
  const fetcher = (url: string) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((x) =>
        parsePagination(
          x,
          ({ verstrektDoorPartij__uuid }: any) =>
            verstrektDoorPartij__uuid as string,
        ),
      )
      .then(async (r) => ({
        page: await Promise.all(
          [...new Set(r.page)].map((u) => fetchSingleKlant(getKlantIdUrl(u))),
        ),
      }));

  return ServiceResult.fromFetcher(getUrl, fetcher);
}

export async function ensureKlantForBsn({
  bsn,
  partijIdentificatie,
}: {
  bsn: string;
  partijIdentificatie: PersoonIdentificatie;
}) {
  if (!bsn) throw new Error();
  const soortPartij = PartijTypes.persoon;
  const identificatorType = identificatorTypes.persoon;
  const url = getUrlForSingleKlantByIdentificator(
    soortPartij,
    identificatorType,
    bsn,
  );

  const klant =
    (await searchSingleKlant(url)) ??
    (await createKlant(
      { partijIdentificatie, soortPartij },
      identificatorType,
      bsn,
    ));

  const idUrl = getKlantIdUrl(klant.id);
  mutate(idUrl, klant);
  mutate(url, klant);

  return klant;
}

const getUrlForSingleKlantByBedrijfIdentifier = (
  bedrijfSearchParameter: BedrijfIdentifier | undefined,
) => {
  if (!bedrijfSearchParameter) {
    return "";
  }

  const soortPartij = PartijTypes.organisatie;

  if (
    "vestigingsnummer" in bedrijfSearchParameter &&
    bedrijfSearchParameter.vestigingsnummer
  )
    return getUrlForSingleKlantByIdentificator(
      soortPartij,
      identificatorTypes.vestiging,
      bedrijfSearchParameter.vestigingsnummer,
    );

  if ("rsin" in bedrijfSearchParameter && bedrijfSearchParameter.rsin) {
    return getUrlForSingleKlantByIdentificator(
      soortPartij,
      identificatorTypes.nietNatuurlijkPersoonRsin,
      bedrijfSearchParameter.rsin,
    );
  }

  return "";
};

export const useKlantByBedrijfIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
) => {
  const getUrl = () => getUrlForSingleKlantByBedrijfIdentifier(getId());
  return ServiceResult.fromFetcher(getUrl, searchSingleKlant);
};

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForBedrijfIdentifier({
  identifier,
  partijIdentificatie,
}: {
  identifier: BedrijfIdentifier;
  partijIdentificatie: OrganisatieIdentificatie;
}) {
  const bedrijfIdentifierUrl =
    getUrlForSingleKlantByBedrijfIdentifier(identifier);

  if (!bedrijfIdentifierUrl) throw new Error();

  const klant =
    (await searchSingleKlant(bedrijfIdentifierUrl)) ??
    (await createBedrijfKlant(identifier, partijIdentificatie));

  if (
    "kvkNummer" in identifier &&
    identifier.kvkNummer &&
    klant.kvkNummer &&
    klant.kvkNummer !== identifier.kvkNummer
  ) {
    throw new Error();
  }

  const idUrl = getKlantIdUrl(klant.id);
  mutate(idUrl, klant);

  return klant;
}

const getUrlForSingleKlantByIdentificator = (
  soortPartij: PartijTypes,
  identificatorType: IdentificatorType,
  objectId: string,
) =>
  klantinteractiesBaseUrl +
  "/partijen?" +
  new URLSearchParams({
    soortPartij,
    partijIdentificator__codeSoortObjectId: identificatorType.codeSoortObjectId,
    partijIdentificator__objectId: objectId,
    expand: "digitaleAdressen",
  });

async function createBedrijfKlant(
  identifier: BedrijfIdentifier,
  partijIdentificatie: OrganisatieIdentificatie,
) {
  const soortPartij = PartijTypes.organisatie;
  if ("vestigingsnummer" in identifier) {
    return createKlant(
      { partijIdentificatie, soortPartij },
      identificatorTypes.vestiging,
      identifier.vestigingsnummer,
    );
  }

  const klant = await createKlant(
    { partijIdentificatie, soortPartij },
    identificatorTypes.nietNatuurlijkPersoonRsin,
    identifier.rsin,
  );

  if (identifier.kvkNummer) {
    await createPartijIdentificator({
      identificeerdePartij: {
        url: klant.url,
        uuid: klant.id,
      },
      partijIdentificator: {
        ...identificatorTypes.nietNatuurlijkPersoonKvkNummer,
        objectId: identifier.kvkNummer,
      },
    });
  }

  return klant;
}

async function mapPartijToKlant(
  partij: Partij,
  identificatoren?: any[],
): Promise<Klant> {
  if (!identificatoren?.length) {
    const promises = partij.partijIdentificatoren.map(({ uuid }) =>
      getPartijIdentificator(uuid),
    );
    identificatoren = await Promise.all(promises);
  }

  const getDigitaalAdres = (type: string) =>
    partij._expand?.digitaleAdressen?.find(
      (x) => x.adres && x.soortDigitaalAdres === type,
    )?.adres;

  const getIdentificator = (type: { codeSoortObjectId: string }) =>
    identificatoren?.find(
      (x) =>
        x?.partijIdentificator?.objectId &&
        x?.partijIdentificator?.codeSoortObjectId == type.codeSoortObjectId,
    )?.partijIdentificator?.objectId;

  return {
    _typeOfKlant: "klant",
    klantnummer: partij.nummer || "",
    id: partij.uuid,
    url: partij.url,
    telefoonnummer: getDigitaalAdres(digitaalAdresTypes.telefoonnummer),
    emailadres: getDigitaalAdres(digitaalAdresTypes.email),
    bsn: getIdentificator(identificatorTypes.persoon),
    vestigingsnummer: getIdentificator(identificatorTypes.vestiging),
    kvkNummer: getIdentificator(
      identificatorTypes.nietNatuurlijkPersoonKvkNummer,
    ),
    rsin: getIdentificator(identificatorTypes.nietNatuurlijkPersoonRsin),
  };
}

const createPartij = ({
  partijIdentificatie,
  soortPartij,
}: CreateKlantParameters): Promise<Partij> =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partijen", {
    body: JSON.stringify({
      digitaleAdressen: [],
      betrokkenen: [],
      categorieRelaties: [],
      voorkeursDigitaalAdres: null,
      vertegenwoordigden: [],
      rekeningnummers: [],
      partijIdentificatoren: [],
      voorkeursRekeningnummer: null,
      indicatieGeheimhouding: false,
      indicatieActief: true,
      partijIdentificatie,
      soortPartij,
    }),
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
  })
    .then(throwIfNotOk)
    .then(parseJson);

const createPartijIdentificator = (body: {
  identificeerdePartij: {
    url: string;
    uuid: string;
  };
  partijIdentificator: IdentificatorType & {
    objectId: string;
  };
}) =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partij-identificatoren", {
    body: JSON.stringify(body),
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
  })
    .then(throwIfNotOk)
    .then(parseJson);

const getPartijIdentificator = (uuid: string) =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partij-identificatoren/" + uuid)
    .then(throwIfNotOk)
    .then(parseJson);

const createKlant = (
  klantParameters: CreateKlantParameters,
  identificatorType: IdentificatorType,
  objectId: string,
): Promise<Klant> =>
  createPartij(klantParameters).then((partij) =>
    createPartijIdentificator({
      identificeerdePartij: {
        url: partij.url,
        uuid: partij.uuid,
      },
      partijIdentificator: {
        ...identificatorType,
        objectId,
      },
    }).then((identificator) => mapPartijToKlant(partij, [identificator])),
  );
