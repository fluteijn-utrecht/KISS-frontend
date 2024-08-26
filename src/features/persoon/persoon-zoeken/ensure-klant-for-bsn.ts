import {
  findKlantByIdentifier,
  createKlant,
  type Contactnaam,
} from "@/services/klanten";

export const ensureKlantForBsn = async (parameters: {
  bsn: string;
  contactnaam: Contactnaam;
}) =>
  (await findKlantByIdentifier(parameters)) ?? (await createKlant(parameters));
