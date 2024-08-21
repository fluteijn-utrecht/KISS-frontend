export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummers: string[];
  emailadressen: string[];
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
