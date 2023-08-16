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
  vestigingsnummer?: string;
  url?: string;
}

export enum KlantType {
  Persoon = "natuurlijk_persoon",
  Bedrijf = "vestiging",
}
