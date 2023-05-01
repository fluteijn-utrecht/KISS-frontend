import {
  fetchLoggedIn,
  parseJson,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
  enforceOneOrZero,
} from "@/services";

import type {
  SearchCategories,
  BedrijfQuery,
  BedrijfQueryDictionary,
  Bedrijf,
} from "./types";

export const bedrijfQuery = <K extends SearchCategories>(
  query: BedrijfQuery<K>
) => query;

type KvkPagination = {
  pagina: number;
  aantal: number;
  totaal: number;
  resultaten: any[];
};

const parseKvkPagination = async ({
  pagina,
  aantal,
  totaal,
  resultaten,
}: KvkPagination): Promise<Paginated<{ vestigingsnummer: string }>> => ({
  page: await Promise.all(resultaten.map(mapHandelsRegister)),
  pageNumber: pagina,
  totalRecords: totaal,
  pageSize: aantal,
  totalPages: totaal === 0 ? 0 : Math.ceil(totaal / aantal),
});

const handelsRegisterBaseUrl = "/api/kvk";

const bedrijfQueryDictionary: BedrijfQueryDictionary = {
  postcodeHuisnummer: ({ postcode, huisnummer }) => [
    ["postcode", postcode.numbers + postcode.digits],
    ["huisnummer", huisnummer],
  ],
  emailadres: (search) => [["emailAdres[like]", search]],
  telefoonnummer: (search) => [["telefoonnummer[like]", search]],
  kvkNummer: (search) => [["kvkNummer", search]],
  handelsnaam: (search) => [["handelsnaam", search]],
};

const getSearchBedrijvenUrl = <K extends SearchCategories>({
  query,
  page,
}: SearchBedrijfArguments<K>) => {
  if (!query?.value) return "";

  const params = new URLSearchParams();
  // TODO: think about how to search in both klantregister and handelsregister for phone / email

  params.set("pagina", page?.toString() ?? "1");

  const searchParams = bedrijfQueryDictionary[query.field](query.value);

  searchParams.forEach((tuple) => {
    params.set(...tuple);
  });

  return `${handelsRegisterBaseUrl}/zoeken?${params}`;
};

async function mapHandelsRegister(json: any): Promise<Bedrijf> {
  const { vestigingsnummer, kvkNummer, handelsnaam, straatnaam, plaats } =
    json ?? {};

  let vestiging: KvkVestiging | undefined;

  if (vestigingsnummer) {
    vestiging = await fetchVestiging(getVestingUrl(vestigingsnummer));
  }

  return {
    _typeOfKlant: "bedrijf",
    kvkNummer,
    vestigingsnummer,
    bedrijfsnaam: handelsnaam,
    straatnaam,
    woonplaats: plaats,
    ...(vestiging ?? {}),
  };
}

type KvkVestiging = {
  vestigingsnummer: string;
  kvkNummer: string;
  handelsnaam: string;
  postcode: string;
  huisnummer: string;
  huisletter: string;
  huisnummertoevoeging: string;
};

const mapVestiging = ({
  vestigingsnummer,
  kvkNummer,
  eersteHandelsnaam,
  adressen,
}: any): KvkVestiging => {
  const { huisnummerToevoeging, postcode, huisnummer, huisletter } =
    adressen?.find((x: any) => x?.type === "bezoekadres") ??
    adressen?.[0] ??
    {};

  return {
    vestigingsnummer,
    kvkNummer,
    handelsnaam: eersteHandelsnaam,
    huisnummertoevoeging: huisnummerToevoeging,
    postcode,
    huisnummer,
    huisletter,
  };
};

function searchBedrijvenInHandelsRegister(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(parseKvkPagination);
}

type SearchBedrijfArguments<K extends SearchCategories> = {
  query: BedrijfQuery<K> | undefined;
  page: number | undefined;
};

export function useSearchBedrijven<K extends SearchCategories>(
  getArgs: () => SearchBedrijfArguments<K>
) {
  return ServiceResult.fromFetcher(
    () => getSearchBedrijvenUrl(getArgs()),
    searchBedrijvenInHandelsRegister
  );
}

const getVestingUrl = (vestigingsnummer?: string) => {
  if (!vestigingsnummer) return "";
  return handelsRegisterBaseUrl + "/vestigingsprofielen/" + vestigingsnummer;
};

const fetchVestiging = (url: string) =>
  fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapVestiging);

export const useBedrijfByVestigingsnummer = (
  getVestigingsnummer: () => string | undefined
) => {
  const getUrl = () => {
    const vestigingsnummer = getVestigingsnummer();
    if (!vestigingsnummer) return "";
    const searchParams = new URLSearchParams();
    searchParams.set("vestigingsnummer", vestigingsnummer);
    return `${handelsRegisterBaseUrl}/zoeken?${searchParams}`;
  };

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  const fetcher = (url: string) =>
    searchBedrijvenInHandelsRegister(url).then(enforceOneOrZero);

  return ServiceResult.fromFetcher(getUrl, fetcher, {
    getUniqueId,
  });
};
