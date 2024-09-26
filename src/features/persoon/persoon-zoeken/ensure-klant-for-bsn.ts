import { findKlantByIdentifier, createKlant } from "@/services/klanten";

export const ensureKlantForBsn = async (parameters: { bsn: string }) =>
  (await findKlantByIdentifier(parameters)) ?? (await createKlant(parameters));
