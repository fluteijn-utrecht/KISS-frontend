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
    ["bezoekadres.postcode", postcode.digits + postcode.numbers],
    ["bezoekadres.straatHuisnummer", huisnummer],
  ],
  emailadres: (search) => [["emails.email", `%${search}%`]],
  telefoonnummer: (search) => [
    ["telefoonnummers.telefoonnummer", `%${search}%`],
  ],
  kvkNummer: (search) => [["kvknummer", search]],
  handelsnaam: (search) => [["eersteHandelsnaam", search]],
};

const getSearchBedrijvenUrl = <K extends SearchCategories>({
  query,
  page,
}: SearchBedrijfArguments<K>) => {
  if (!query?.value) return "";

  const url = new URL(handelsRegisterBaseUrl);
  // TODO: think about how to search in both klantregister and handelsregister for phone / email

  url.searchParams.set("page", page?.toString() ?? "1");
  url.searchParams.set("extend[]", "all");

  const searchParams = bedrijfQueryDictionary[query.field](query.value);

  searchParams.forEach((tuple) => {
    // url.searchParams.set(...tuple);
  });

  return url.toString();
};

function mapHandelsRegister(json: any): Bedrijf {
  const {
    bezoekadres,
    emailAdres,
    telefoonnummer,
    vestigingsnummer,
    kvknummer,
    eersteHandelsnaam,
  } = json ?? {};

  const { straatHuisnummer, postcode } = bezoekadres ?? {};

  return {
    _typeOfKlant: "bedrijf",
    kvknummer,
    vestigingsnummer,
    postcode,
    huisnummer: straatHuisnummer,
    telefoonnummer,
    email: emailAdres,
    bedrijfsnaam: eersteHandelsnaam,
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

export const useBedrijfHandelsregisterByVestigingsnummer = (
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