import {
  findKlantByIdentifier,
  createKlant,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1 } from "@/services/openklant1/service";
import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user";
import { getRegisterDetails as getSysteemDetails } from "@/features/shared/systeemdetails";

export const ensureKlantForBedrijfIdentifier = async (
  klantbedrijfidentifier: KlantBedrijfIdentifier,
  bedrijfsnaam: string,
) => {
  const { useKlantInteractiesApi, systeemId } = await getSysteemDetails();

  if (useKlantInteractiesApi) {
    const klant = await findKlantByIdentifier(
      systeemId,
      klantbedrijfidentifier,
    );
    return klant ?? (await createKlant(systeemId, klantbedrijfidentifier));
  } else {
    const mappedIdentifier = mapBedrijfsIdentifier(klantbedrijfidentifier);
    const organisatieIds = useOrganisatieIds();
    const organisatieId = organisatieIds.value[0] || "";

    return await ensureKlantForBedrijfIdentifierOk1(
      systeemId,
      { bedrijfsnaam, identifier: mappedIdentifier },
      organisatieId,
    );
  }
};
