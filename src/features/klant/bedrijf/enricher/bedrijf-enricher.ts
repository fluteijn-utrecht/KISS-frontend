import type { Klant } from "../../types";
import { useKlantByIdentifier } from "../../service";
import { ServiceResult, combineEnrichers } from "@/services";
import { useBedrijfByIdentifier } from "../service/use-bedrijf-by-identifier";
import type { Bedrijf, BedrijfIdentifier } from "../types";
import {
  NietNatuurlijkPersoonIdentifiers,
  fetchPreferredNietNatuurlijkPersoonIdentifier,
} from "../service/fetch-preferred-niet-natuurlijk-persoon-identifier";

const isKlant = (klantOfBedrijf: Klant | Bedrijf): klantOfBedrijf is Klant => {
  return klantOfBedrijf._typeOfKlant === "klant";
};

export function useEnrichedBedrijf(getKlantOrBedrijf: () => Klant | Bedrijf) {
  //dit hebben we alleen nodig als we niet op vestigingsnummer maar een andere identifier
  //moeten matchen. Door incompatabiliteit tussen esuite en openklant
  //moet de gebruikte matching gegeven configurabel zijn.
  const preferredNietNatuurlijkPersoonIdentifier = ServiceResult.fromFetcher(
    "niet-natuurlijk-persoon-identifier",
    fetchPreferredNietNatuurlijkPersoonIdentifier,
  );

  function getSharedIdentifier(
    klantofbedrijf: Klant | Bedrijf,
  ): BedrijfIdentifier | undefined {
    //als er een vestigingsnummer is dan is dat mooi
    //zowel kvk als de klantregisters kennen dit
    if (klantofbedrijf.vestigingsnummer) {
      return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
    }

    if (!preferredNietNatuurlijkPersoonIdentifier.success) return undefined;

    if (
      preferredNietNatuurlijkPersoonIdentifier.data
        .nietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.kvkNummer &&
      klantofbedrijf.kvkNummer
    ) {
      return {
        kvkNummer: klantofbedrijf.kvkNummer,
      };
    }

    const rsin =
      "rsin" in klantofbedrijf ? klantofbedrijf.rsin : klantofbedrijf.innNnpId;

    if (
      preferredNietNatuurlijkPersoonIdentifier.data
        .nietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.rsin &&
      rsin
    ) {
      return {
        rsin,
      };
    }

    return;
  }

  //zoekt een bedrijf in twee bronnen en combineert het resultaat.
  const enricher = combineEnrichers(
    useKlantByIdentifier,
    useBedrijfByIdentifier,
    getSharedIdentifier, //een functie die de property waarmee je het bijbehorende object in de andere bron gaat zoeken
    isKlant,
  );

  return enricher(getKlantOrBedrijf);
}
