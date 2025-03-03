import type { KlantIdentificator } from "./types";

export const getIdentificatorForOk1And2 = ({
  bsn,
  vestigingsnummer,
  rsin,
  kvkNummer,
}: KlantIdentificator) => {
  switch (true) {
    case !!bsn:
      return { ok1: { bsn }, ok2: { bsn } } as const;
    case !!vestigingsnummer:
      return {
        ok1: { vestigingsnummer },
        ok2: { vestigingsnummer },
      } as const;
    case !!kvkNummer:
      // esuite wil een kvkNummer als niet-natuurlijk-persoon-Id
      return {
        ok1: { nietNatuurlijkPersoonIdentifier: kvkNummer },
        ok2: { kvkNummer, rsin },
      };
    case !!rsin:
      // esuite kan niks met een rsin
      return { ok2: { rsin } };

    default:
      return {};
  }
};
