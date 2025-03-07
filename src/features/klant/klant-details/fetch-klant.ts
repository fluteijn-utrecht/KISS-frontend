import {
  fetchKlantByIdOk2,
  findKlantByIdentifierOpenKlant2,
} from "@/services/openklant2";
import {
  fetchKlantByIdOk1,
  fetchKlantByIdentifierOpenKlant1,
} from "@/services/openklant1";
import {
  registryVersions,
  useSystemen,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";

const { systemen, defaultSysteem } = useSystemen();

export const fetchKlant = async ({
  id,
}: {
  id: string;
}): Promise<Klant | null> => {
  const klant = await fetchKlantById(id, defaultSysteem.value);

  if (heeftContactgegevens(klant)) return klant;
  if (!systemen.value?.length) return klant;

  for (const systeem of systemen.value.filter(
    (s) => s.identifier !== defaultSysteem.value.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(klant, systeem);
    if (heeftContactgegevens(fallbackKlant)) return fallbackKlant;
  }

  return klant;
};

const fetchKlantByNonDefaultSysteem = async (
  klant: Klant | null,
  systeem: Systeem,
): Promise<Klant | null> => {
  if (!klant) return null;

  const identifier = getIdentifier(klant);
  if (!identifier) return klant;

  const gevondenKlant =
    systeem.registryVersion === registryVersions.ok1
      ? await fetchKlantByIdentifierOpenKlant1(systeem.identifier, identifier)
      : await findKlantByIdentifierOpenKlant2(systeem.identifier, identifier);

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

const getIdentifier = (klant: Klant) =>
  klant.bsn
    ? { bsn: klant.bsn }
    : klant.vestigingsnummer && klant.nietNatuurlijkPersoonIdentifier
      ? {
          vestigingsnummer: klant.vestigingsnummer,
          nietNatuurlijkPersoonIdentifier:
            klant.nietNatuurlijkPersoonIdentifier,
        }
      : null;
