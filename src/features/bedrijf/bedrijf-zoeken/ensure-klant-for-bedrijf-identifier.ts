import type { BedrijfIdentifier } from "@/services/kvk";
import { findKlantByIdentifier, createKlant } from "@/services/klanten";

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export const ensureKlantForBedrijfIdentifier = async (
  identifier: BedrijfIdentifier,
  naam: string,
) =>
  (await findKlantByIdentifier(identifier)) ??
  (await createKlant({ ...identifier, naam }));
