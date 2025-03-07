import { fetchKlantByIdOk2 } from "@/services/openklant2";
import {
  fetchKlantByIdOk1,
  searchSingleKlant,
  getKlantBsnUrl,
} from "@/services/openklant1";
import {
  registryVersions,
  useSystemen,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";

const systemenInfo = useSystemen();

export const fetchKlant = async ({
  id,
}: {
  id: string;
}): Promise<Klant | null> => {
  const klant = await fetchKlantById(id, systemenInfo.defaultSysteem.value);

  if (klant?.emailadressen?.length || klant?.telefoonnummers?.length) {
    return klant;
  }
  if (!systemenInfo.systemen.value || !systemenInfo.systemen.value.length) {
    return klant;
  }

  const nonDefaultSystemen = systemenInfo.systemen.value.filter(
    (x) => x.identifier !== systemenInfo.defaultSysteem.value.identifier,
  );

  for (const systeem of nonDefaultSystemen) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(klant, systeem);

    if (
      fallbackKlant?.emailadressen?.length ||
      fallbackKlant?.telefoonnummers?.length
    ) {
      console.log(
        "Fallback-klant gevonden met contactgegevens:",
        fallbackKlant,
      );
      return fallbackKlant;
    }
  }

  console.error("Geen fallback-klant met contactgegevens gevonden.");
  return klant;
};

// Haalt een klant op uit een non-default systeem en bepaalt de juiste methode (BSN of ID)
const fetchKlantByNonDefaultSysteem = async (
  klant: Klant | null,
  systeem: Systeem,
): Promise<Klant | null> => {
  console.log(`Controleer fallback-systeem ${systeem.identifier}...`);

  if (systeem.registryVersion === registryVersions.ok1 && klant?.bsn) {
    const bsnUrl = getKlantBsnUrl(klant.bsn);
    const gevondenKlant = await searchSingleKlant(systeem.identifier, bsnUrl);

    if (gevondenKlant) {
      console.log(`Klant-ID via BSN gevonden: ${gevondenKlant.id}`);
      return fetchKlantById(gevondenKlant.id, systeem); // Haal de klant met het nieuwe ID op
    }

    console.error("Geen klant gevonden via BSN.");
    return null;
  }

  // Als het een OK2-systeem is, haal de klant direct op met het originele ID
  return null;
};

// Haalt een klant op uit het systeem met een gegeven ID
const fetchKlantById = async (
  id: string,
  systeem: Systeem,
): Promise<Klant | null> => {
  const fetchKlant =
    systeem.registryVersion === registryVersions.ok1
      ? fetchKlantByIdOk1
      : fetchKlantByIdOk2;

  console.log(`Ophalen klant van ${systeem.identifier} met ID: ${id}`);

  try {
    const klant = await fetchKlant(systeem.identifier, id);
    console.log(`Klant opgehaald van ${systeem.identifier}:`, klant);
    return klant;
  } catch (error) {
    console.error(`Fout bij ophalen klant uit ${systeem.identifier}:`, error);
    return null;
  }
};
