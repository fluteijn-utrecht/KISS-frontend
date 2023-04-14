import {
  enforceOneOrZero,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
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

const handelsRegisterBaseUrl = window.gatewayBaseUri + "/api/vestigingen";

const bedrijfQueryDictionary: BedrijfQueryDictionary = {
  postcodeHuisnummer: ({ postcode, huisnummer }) => [
    ["embedded.bezoekadres.postcode", postcode.numbers + postcode.digits],
    ["embedded.bezoekadres.huisnummer[int_compare]", huisnummer],
  ],
  emailadres: (search) => [["emailAdres[like]", search]],
  telefoonnummer: (search) => [["telefoonnummer[like]", search]],
  kvkNummer: (search) => [["kvkNummer", search]],
  handelsnaam: (search) => [["eersteHandelsnaam[like]", search]],
};

const getSearchBedrijvenUrl = <K extends SearchCategories>({
  query,
  page,
}: SearchBedrijfArguments<K>) => {
  if (!query?.value) return "";

  const url = new URL(handelsRegisterBaseUrl);
  // TODO: think about how to search in both klantregister and handelsregister for phone / email

  url.searchParams.set("_page", page?.toString() ?? "1");
  url.searchParams.set("extend[]", "all");

  const searchParams = bedrijfQueryDictionary[query.field](query.value);

  searchParams.forEach((tuple) => {
    url.searchParams.set(...tuple);
  });

  return url.toString();
};

function mapHandelsRegister(json: any): Bedrijf {
  const {
    embedded,
    emailAdres,
    telefoonnummer,
    vestigingsnummer,
    kvkNummer,
    eersteHandelsnaam,
  } = json ?? {};

  const {
    huisnummer,
    postcode,
    straatnaam,
    huisletter,
    huisnummertoevoeging,
    woonplaats,
  } = embedded?.bezoekadres ?? {};

  return {
    _typeOfKlant: "bedrijf",
    kvkNummer,
    vestigingsnummer,
    postcode,
    huisnummer: huisnummer.toString(),
    telefoonnummer,
    email: emailAdres,
    bedrijfsnaam: eersteHandelsnaam,
    straatnaam,
    huisletter,
    huisnummertoevoeging,
    woonplaats,
  };
}

function searchBedrijvenInHandelsRegister(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapHandelsRegister));
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

export const useBedrijfByVestigingsnummer = (
  getVestigingsnummer: () => string | undefined
) => {
  const getUrl = () => {
    const vestigingsnummer = getVestigingsnummer();
    if (!vestigingsnummer) return "";
    const url = new URL(handelsRegisterBaseUrl);
    url.searchParams.set("extend[]", "all");
    url.searchParams.set("vestigingsnummer", vestigingsnummer);
    return url.toString();
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
