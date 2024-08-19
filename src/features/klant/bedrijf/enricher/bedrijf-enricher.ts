import type { Klant } from "../../types";
import { useKlantByBedrijfIdentifier } from "../../service";
import { combineEnrichers } from "@/services";
import { useBedrijfByIdentifier } from "../service/use-bedrijf-by-identifier";
import type { Bedrijf, BedrijfIdentifier } from "../types";

const isKlant = (klantOfBedrijf: Klant | Bedrijf): klantOfBedrijf is Klant => {
  return klantOfBedrijf._typeOfKlant === "klant";
};

function getSharedIdentifier(
  klantofbedrijf: Klant | Bedrijf,
): BedrijfIdentifier | undefined {
  //als er een vestigingsnummer is dan is dat mooi
  //zowel kvk als de klantregisters kennen dit
  if (klantofbedrijf.vestigingsnummer) {
    return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
  }

  if (klantofbedrijf.rsin) {
    return {
      rsin: klantofbedrijf.rsin,
    };
  }
}

export const useEnrichedBedrijf = combineEnrichers(
  useKlantByBedrijfIdentifier,
  useBedrijfByIdentifier,
  getSharedIdentifier, //een functie die de property waarmee je het bijbehorende object in de andere bron gaat zoeken
  isKlant,
);
