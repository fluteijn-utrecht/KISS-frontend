import type { ServiceData } from "@/services";

export interface Bedrijf {
  _typeOfKlant: "bedrijf";
  kvkNummer: string;
  type: string;
  vestigingsnummer?: string;
  bedrijfsnaam: string;
  postcode?: string;
  huisnummer?: string;
  straatnaam: string;
  huisletter?: string;
  huisnummertoevoeging?: string;
  woonplaats?: string;
  innNnpId?: string;
  rsin?: string;
}

export interface EnrichedBedrijf {
  bedrijfsnaam: ServiceData<string>;
  type: ServiceData<string>;
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
};

export type SearchCategories = keyof SearchCategoryTypes;

export type BedrijfQueryDictionary = {
  [K in SearchCategories]: (
    search: SearchCategoryTypes[K],
  ) => readonly [string, string][];
};

export type BedrijfQuery<K extends SearchCategories = SearchCategories> = {
  field: K;
  value: SearchCategoryTypes[K];
};

export type BedrijfIdentifier =
  | {
      vestigingsnummer: string;
    }
  | {
      kvkNummer: string;
    }
  | {
      rsin: string;
    };
