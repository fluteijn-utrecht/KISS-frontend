export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummer?: string;
  emailadres?: string;
  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string;
  rsin?: string;
  kvkNummer?: string;
  url: string;
}

export type KlantBedrijfIdentifier =
  | {
      vestigingsnummer: string;
    }
  | {
      rsin: string;
    };
