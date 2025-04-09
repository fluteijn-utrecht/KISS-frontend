import {
  fetchKlantByIdOk2,
  fetchKlantByKlantIdentificatorOk2,
} from "@/services/openklant2";
import {
  fetchKlantByIdOk1,
  fetchKlantByKlantIdentificatorOk1,
} from "@/services/openklant1";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";
import { mapKlantToKlantIdentifier } from "@/features/contact/shared";

export const fetchKlant = async ({
  id,
  systemen,
  defaultSysteem,
}: {
  id: string;
  systemen: Systeem[];
  defaultSysteem: Systeem;
}): Promise<Klant | null> => {
  const klant = await fetchKlantById(id, defaultSysteem);

  if (heeftContactgegevens(klant)) return klant;
  if (!systemen.length) return klant;

  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      klant,
      nonDefaultSysteem,
    );

    //we nemen alleen de contactgegevens over als die niet in de default klant zitten, maar wel in een ander system zijn gevonden
    //alleen de contactgegevens, geen andere gegevens overnemen, de klant uit het default systeem is leidend!
    if (klant && fallbackKlant && heeftContactgegevens(fallbackKlant)) {
      klant.telefoonnummer = fallbackKlant.telefoonnummer;
      klant.telefoonnummers = fallbackKlant.telefoonnummers;
      klant.emailadres = fallbackKlant.emailadres;
      klant.emailadressen = fallbackKlant.emailadressen;
    }
  }

  return klant;
};

const fetchKlantByNonDefaultSysteem = async (
  klant: Klant | null,
  systeem: Systeem,
): Promise<Klant | null> => {
  if (!klant) return null;

  const identifier = mapKlantToKlantIdentifier(systeem.registryVersion, klant);
  if (!identifier) return klant;

  const gevondenKlant =
    systeem.registryVersion === registryVersions.ok1
      ? await fetchKlantByKlantIdentificatorOk1(systeem.identifier, identifier)
      : await fetchKlantByKlantIdentificatorOk2(systeem.identifier, identifier);

  return gevondenKlant ? fetchKlantById(gevondenKlant.id, systeem) : klant;
};

const fetchKlantById = async (
  id: string,
  systeem: Systeem,
): Promise<Klant | null> => {
  try {
    return await (
      systeem.registryVersion === registryVersions.ok1
        ? fetchKlantByIdOk1
        : fetchKlantByIdOk2
    )(systeem.identifier, id);
  } catch {
    return null;
  }
};

const heeftContactgegevens = (klant: Klant | null) =>
  klant?.emailadressen?.length || klant?.telefoonnummers?.length;
