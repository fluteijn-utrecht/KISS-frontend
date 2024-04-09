export type UpdateContactgegevensParams = Pick<
  Klant,
  "id" | "telefoonnummer" | "emailadres"
>;

export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummer?: string;
  emailadres?: string;
  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string; //als de klant een Kvk klant is, dan gebruiken we dit gegeven om het klantenregistern en de kvk te kunnen matchen. Als het een bedrijf betreft
  url?: string;
  innNnpId?: string; //als de klant een Kvk klant is, dan gebruiken we dit gegeven om het klantenregistern en de kvk te kunnen matchen. Als het een kvk entiteit zonder vestiging betreft
  kvkNummer?: string; //todo: uitzoeken waarom niet dit ipv innNnpId > als de klant een Kvk klant is, dan gebruiken we dit gegeven om het klantenregistern en de kvk te kunnen matchen. Als het een kvk entiteit zonder vestiging betreft
}

export enum KlantType {
  Persoon = "natuurlijk_persoon",
  Bedrijf = "vestiging",
  NietNatuurlijkPersoon = "niet_natuurlijk_persoon",
}

export interface Contactverzoek {
  contactmoment: string;
}
