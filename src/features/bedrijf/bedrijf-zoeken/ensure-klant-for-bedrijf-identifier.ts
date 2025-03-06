import {
  type KlantBedrijfIdentifier,
  ensureOk2Klant,
} from "@/services/openklant2";
import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1 } from "@/services/openklant1/service";
import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user";
import {
  registryVersions,
  useSystemen,
} from "@/services/environment/fetch-systemen";

const { defaultSysteem } = useSystemen();

export const ensureKlantForBedrijfIdentifier = async (
  klantbedrijfidentifier: KlantBedrijfIdentifier,
  bedrijfsnaam: string,
) => {
  if (!defaultSysteem.value) {
    throw new Error("Geen default register gevonden");
  }

  if (defaultSysteem.value.registryVersion === registryVersions.ok2) {
    return await ensureOk2Klant(
      defaultSysteem.value.identifier,
      klantbedrijfidentifier,
    );
  } else {
    const mappedIdentifier = mapBedrijfsIdentifier(klantbedrijfidentifier);
    const organisatieIds = useOrganisatieIds();
    const organisatieId = organisatieIds.value[0] || "";

    return await ensureKlantForBedrijfIdentifierOk1(
      defaultSysteem.value.identifier,
      { bedrijfsnaam, identifier: mappedIdentifier },
      organisatieId,
    );
  }
};
