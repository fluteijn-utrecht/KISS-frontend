import type { ServiceData } from "@/services";

export interface Persoon {
  _typeOfKlant: "persoon";
  bsn: string;
  geboortedatum?: Date;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  geboorteplaats?: string;
  geboorteland?: string;
  postcode?: string;
  huisnummer?: string;
  straat?: string;
  woonplaats?: string;
  huisletter?: string;
  huisnummertoevoeging?: string;
}

export interface EnrichedPersoon {
  naam: ServiceData<string | null>;
  bsn: string | undefined;
  telefoonnummers: ServiceData<string | null>;
  emails: ServiceData<string | null>;
  geboortedatum: ServiceData<Date | null | undefined>;
  postcodeHuisnummer: ServiceData<string | null>;
  create: () => Promise<void>;
  detailLink: ServiceData<{
    to: string;
    title: string;
  } | null>;
}
