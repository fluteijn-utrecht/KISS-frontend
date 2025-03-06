import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";
import {
  registryVersions,
  useSystemen,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";

const systemenInfo = useSystemen();

// Haalt een klant op uit het default systeem. Als er geen e-mail en geen telefoonnummer is, probeert het andere systemen totdat er een geldige klant wordt gevonden.
export const fetchKlantById = async ({
  id,
}: {
  id: string;
}): Promise<Klant | null> => {
  const klant = await fetchKlantBySysteem(
    id,
    systemenInfo.defaultSysteem.value,
  );

  const mistContactgegevens =
    !klant?.emailadressen?.length && !klant?.telefoonnummers?.length;

  if (mistContactgegevens && systemenInfo.systemen.value) {
    //if the default system does not contain contactinfo,
    //try to find contactinfo in any of the other systems

    const nonDefaultSystemen = systemenInfo.systemen.value.filter(
      (x) => x.identifier != systemenInfo.defaultSysteem.value.identifier,
    );

    for (const systeem of nonDefaultSystemen) {
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
