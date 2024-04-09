import type { Klant } from "../../types";
import { useKlantByIdentifier } from "../../service";
import { combineEnrichers } from "@/services";
import { useBedrijfByVestigingsnummer } from "../service/UseGetOndernememing";
import type { Bedrijf } from "../types";

const isKlant = (klantOfBedrijf: Klant | Bedrijf): klantOfBedrijf is Klant => {
  return klantOfBedrijf._typeOfKlant === "klant";
};

export const useEnrichedBedrijf = combineEnrichers(
  useKlantByIdentifier,
  useBedrijfByVestigingsnummer,
  GetSharedIdentifier, //de property waarmee je het bijbehorende object in de andere bron gaat zoeken
  isKlant,
);

function GetSharedIdentifier(
  klantofbedrijf: Klant | Bedrijf,
): BedrijfSearchParameter | undefined {
  if (klantofbedrijf.vestigingsnummer) {
    return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
  } else if (klantofbedrijf.innNnpId) {
    return { innNnpId: klantofbedrijf.innNnpId };
  } else if (klantofbedrijf.kvkNummer) {
    return { kvkNummer: klantofbedrijf.kvkNummer };
  }
  return;
}

export type BedrijfSearchParameter =
  | {
      vestigingsnummer: string;
    }
  | {
      kvkNummer: string;
    }
  | {
      innNnpId: string;
    };
