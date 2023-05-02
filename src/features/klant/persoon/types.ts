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
  adresregel1?: string;
  adresregel2?: string;
  adresregel3?: string;
}

export interface EnrichedPersoon {
  naam: ServiceData<string | null>;
  bsn: string | undefined;
  telefoonnummers: ServiceData<string | null>;
  emails: ServiceData<string | null>;
  geboortedatum: ServiceData<Date | null | undefined>;
  adresregel1: ServiceData<string | null>;
  adresregel2: ServiceData<string | null>;
  adresregel3: ServiceData<string | null>;
  create: () => Promise<void>;
  detailLink: ServiceData<{
    to: string;
    title: string;
  } | null>;
}
