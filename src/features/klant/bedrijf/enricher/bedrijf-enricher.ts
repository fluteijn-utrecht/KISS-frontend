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
  GetId, //de property waarmee je het bijbehorende object in de andere bron gaat zoeken
  isKlant,
);

function GetId(
  klantofbedrijf: Klant | Bedrijf,
): BedrijfSearchParameter | undefined {
  if (klantofbedrijf.vestigingsnummer) {
    return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
  } else if (klantofbedrijf.kvkNummer) { toch moor dat ID??
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
