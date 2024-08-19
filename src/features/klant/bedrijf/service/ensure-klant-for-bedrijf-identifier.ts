import { findKlantByIdentifier, createKlant } from "../../service";
import { mutate } from "swrv";
import type { BedrijfIdentifier } from "../types";

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForBedrijfIdentifier(
  parameters: BedrijfIdentifier & { naam: string },
) {
  const klant =
    (await findKlantByIdentifier(parameters)) ??
    (await createKlant(parameters));

  // cache alvast de klant obv de id, zodat we deze al hebben als we naar de detailpagina navigeren
  mutate(klant.id, klant);

  return klant;
}
