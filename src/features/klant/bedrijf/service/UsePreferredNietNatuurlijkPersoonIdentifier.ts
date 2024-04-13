import { ServiceResult, fetchLoggedIn, throwIfNotOk } from "@/services";

export async function usePreferredNietNatuurlijkPersoonIdentifier() {
  // return ServiceResult.fromPromise<NietNatuurlijkPersoonIdentifier>(
  return await fetchLoggedIn("/api/GetNietNatuurlijkPersoonIdentifier")
    .then(throwIfNotOk)
    .then((r) => r.json())
    .then((r: NietNatuurlijkPersoonIdentifier) => r);
  //  );
}

export type NietNatuurlijkPersoonIdentifier = {
  nietNatuurlijkPersoonIdentifier: string;
};

export const NietNatuurlijkPersoonIdentifiers = {
  rsin: "rsin",
  kvkNummer: "kvkNummer",
};
