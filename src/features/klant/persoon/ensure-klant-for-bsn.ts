import {
  findKlantByIdentifier,
  createKlant,
  type Contactnaam,
} from "../service";
import { mutate } from "swrv";

export async function ensureKlantForBsn(parameters: {
  bsn: string;
  contactnaam: Contactnaam;
}) {
  const klant =
    (await findKlantByIdentifier(parameters)) ??
    (await createKlant(parameters));

  // cache alvast de klant obv de id, zodat we deze al hebben als we naar de detailpagina navigeren
  mutate(klant.id, klant);

  return klant;
}
