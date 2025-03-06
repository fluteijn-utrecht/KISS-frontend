import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";
import {
  fetchSystemen,
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";

// Haalt een klant op uit het default systeem. Als er geen e-mail en geen telefoonnummer is, probeert het andere systemen totdat er een geldige klant wordt gevonden.
export const fetchKlantById = async ({
  id,
}: {
  id: string;
}): Promise<Klant | null> => {
  const systemen = await fetchSystemen();

  const defaultSysteem = systemen.find((s) => s.isDefault);
  if (!defaultSysteem) throw new Error("Geen default systeem gevonden");

  const klant = await fetchKlantBySysteem(id, defaultSysteem);

  const mistContactgegevens =
    !klant?.emailadressen?.length && !klant?.telefoonnummers?.length;

  if (mistContactgegevens) {
    for (const systeem of systemen) {
      if (systeem.identifier === defaultSysteem.identifier) continue;

      const fallbackKlant = await fetchKlantBySysteem(id, systeem);

      if (
        fallbackKlant?.emailadressen?.length ||
        fallbackKlant?.telefoonnummers?.length
      ) {
        return fallbackKlant;
      }
    }
  }
  return klant;
};

const fetchKlantBySysteem = async (
  id: string,
  systeem: Systeem,
): Promise<Klant | null> => {
  const fetchKlant =
    systeem.registryVersion === registryVersions.ok1
      ? fetchKlantByIdOk1
      : fetchKlantByIdOk2;

  try {
    return await fetchKlant(systeem.identifier, id);
  } catch (_error) {
    return null;
  }
};
