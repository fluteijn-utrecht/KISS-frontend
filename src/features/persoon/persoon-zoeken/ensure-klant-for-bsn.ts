import { ensureKlantForBsn as ensureKlantForBsn1 } from "@/services/openklant1";
import {
  findKlantByIdentifier,
  createKlant as createKlant2,
} from "@/services/openklant2";
import {
  fetchSystemen,
  klantinteractieVersions,
} from "@/services/environment/fetch-systemen";
import { useOrganisatieIds } from "@/stores/user";

export const ensureKlantForBsn = async (parameters: { bsn: string }) => {
  const systemen = await fetchSystemen();
  const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

  if (!defaultSysteem) {
    throw new Error("Geen default register gevonden");
  }

  const useKlantInteractiesApi =
    defaultSysteem.klantinteractieVersion === klantinteractieVersions.ok2;

  if (useKlantInteractiesApi) {
    // Gebruik openklant2 implementatie
    return (
      (await findKlantByIdentifier(parameters)) ??
      (await createKlant2(parameters))
    );
  } else {
    // Gebruik openklant1 implementatie
    const organisatieIds = useOrganisatieIds();
    const bronorganisatie = organisatieIds.value[0] || "";

    return await ensureKlantForBsn1(parameters, bronorganisatie);
  }
};
