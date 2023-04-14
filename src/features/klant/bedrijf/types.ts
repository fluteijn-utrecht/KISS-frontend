import type { ServiceData } from "@/services";

export interface Bedrijf {
  _typeOfKlant: "bedrijf";
  kvkNummer: string;
  vestigingsnummer: string;
  bedrijfsnaam: string;
  postcode: string;
  huisnummer: string;
  telefoonnummer: string;
  email: string;
  straatnaam: string;
  huisletter: string;
  huisnummertoevoeging: string;
  woonplaats: string;
}

export interface EnrichedBedrijf {
  bedrijfsnaam: ServiceData<string>;
  kvkNummer: ServiceData<string>;
  postcodeHuisnummer: ServiceData<string>;
  email: ServiceData<string>;
  telefoonnummer: ServiceData<string>;
  create: () => Promise<void>;
  detailLink: ServiceData<{
    to: string;
    title: string;
  } | null>;
}

import type { PostcodeHuisnummer } from "@/helpers/validation";

export type SearchCategoryTypes = {
  handelsnaam: string;
  kvkNummer: string;
  postcodeHuisnummer: PostcodeHuisnummer;
  emailadres: string;
  telefoonnummer: string;
};

export type SearchCategories = keyof SearchCategoryTypes;

export type BedrijfQueryDictionary = {
  [K in SearchCategories]: (
    search: SearchCategoryTypes[K]
  ) => readonly [string, string][];
};

export type BedrijfQuery<K extends SearchCategories = SearchCategories> = {
  field: K;
  value: SearchCategoryTypes[K];
};
