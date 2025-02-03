import {
  findKlantByIdentifier,
  createKlant,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1 } from "@/services/openklant1/service";
import {
  fetchSystemen,
  klantinteractieVersions,
} from "@/services/environment/fetch-systemen";
import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user";

export const ensureKlantForBedrijfIdentifier = async (
  identifier: KlantBedrijfIdentifier,
  bedrijfsnaam: string,
) => {
  const systemen = await fetchSystemen();
  const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

  if (!defaultSysteem) {
    throw new Error("Geen default register gevonden");
  }

  const useKlantInteractiesApi =
    defaultSysteem.klantinteractieVersion === klantinteractieVersions.ok2;

  if (useKlantInteractiesApi) {
    // Gebruik openklant2 implementatie
    const klant = await findKlantByIdentifier(identifier);
    return klant ?? (await createKlant(identifier));
  } else {
    // Gebruik openklant1 implementatie
    const mappedIdentifier = mapBedrijfsIdentifier(identifier);
    const organisatieIds = useOrganisatieIds();
    const organisatieId = organisatieIds.value[0] || "";

    return await ensureKlantForBedrijfIdentifierOk1(
      {
        bedrijfsnaam,
        identifier: mappedIdentifier,
      },
      organisatieId,
    );
  }
};
