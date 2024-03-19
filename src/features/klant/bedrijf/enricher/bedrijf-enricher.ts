import type { Klant } from "../../types";
import { useKlantByVestigingsnummer } from "../../service";
import { combineEnrichers } from "@/services";
import { useBedrijfByVestigingsnummer } from "../service/UseGetOndernememing";
import type { Bedrijf } from "../types";

const isKlant = (klantOfBedrijf: Klant | Bedrijf): klantOfBedrijf is Klant => {
  return klantOfBedrijf._typeOfKlant === "klant";
};

export const useEnrichedBedrijf = combineEnrichers(
  useKlantByVestigingsnummer,
  useBedrijfByVestigingsnummer,
  (either) => either.vestigingsnummer,
  isKlant
);
