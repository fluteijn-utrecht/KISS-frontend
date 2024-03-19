import { ServiceResult } from "@/services";

import type {
  SearchCategories,
  BedrijfQuery,
  BedrijfQueryDictionary,
} from "../types";
import { searchBedrijvenInHandelsRegister, zoekenUrl } from "./shared/shared";

export function useSearchBedrijven<K extends SearchCategories>(
  getArgs: () => SearchBedrijfArguments<K>,
) {
  return ServiceResult.fromFetcher(
    () => getSearchBedrijvenUrl(getArgs()),
    searchBedrijvenInHandelsRegister,
  );
}

// useSearchBedrijven private ///////////////////////////////////////////

const getSearchBedrijvenUrl = <K extends SearchCategories>({
  query,
  page,
}: SearchBedrijfArguments<K>) => {
  if (!query?.value) return "";

  const params = new URLSearchParams();
  params.set("pagina", page?.toString() ?? "1");
  params.set("type", "hoofdvestiging");
  params.append("type", "nevenvestiging");

  const searchParams = bedrijfQueryDictionary[query.field](query.value);

  searchParams.forEach((tuple) => {
    params.set(...tuple);
  });

  return `${zoekenUrl}?${params}`;
};

type SearchBedrijfArguments<K extends SearchCategories> = {
  query: BedrijfQuery<K> | undefined;
  page: number | undefined;
};

const bedrijfQueryDictionary: BedrijfQueryDictionary = {
  postcodeHuisnummer: ({ postcode, huisnummer }) => [
    ["postcode", postcode.numbers + postcode.digits],
    ["huisnummer", huisnummer],
  ],
  kvkNummer: (search) => [["kvkNummer", search]],
  handelsnaam: (search) => [["naam", search]],
};
