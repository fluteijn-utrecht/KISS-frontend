import type { BedrijfIdentifier } from "@/services/kvk";
import {
  findKlantByIdentifier,
  createKlant,
} from "../../../services/klanten/service";

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export const ensureKlantForBedrijfIdentifier = async (
  parameters: BedrijfIdentifier & { naam: string },
) =>
  (await findKlantByIdentifier(parameters)) ?? (await createKlant(parameters));
