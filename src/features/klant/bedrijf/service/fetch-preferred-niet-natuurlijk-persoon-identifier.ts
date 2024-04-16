import { fetchLoggedIn, throwIfNotOk } from "@/services";

const url = "/api/GetNietNatuurlijkPersoonIdentifier";

export function fetchPreferredNietNatuurlijkPersoonIdentifier() {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json())
    .then((r: NietNatuurlijkPersoonIdentifier) => r);
}

export type NietNatuurlijkPersoonIdentifier = {
  nietNatuurlijkPersoonIdentifier: string;
};

export const NietNatuurlijkPersoonIdentifiers = {
  rsin: "rsin",
  kvkNummer: "kvkNummer",
} as const;
