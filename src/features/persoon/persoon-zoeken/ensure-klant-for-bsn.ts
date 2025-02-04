import { ensureKlantForBsn as ensureKlantForBsn1 } from "@/services/openklant1";
import {
  findKlantByIdentifier,
  createKlant as createKlant2,
} from "@/services/openklant2";
import { useOrganisatieIds } from "@/stores/user";
import { getRegisterDetails } from "@/features/shared/systeemdetails";

export const ensureKlantForBsn = async (parameters: { bsn: string }) => {
  const { useKlantInteractiesApi, systeemId } = await getRegisterDetails();

  if (useKlantInteractiesApi) {
    return (
      (await findKlantByIdentifier(systeemId, parameters)) ??
      (await createKlant2(systeemId, parameters))
    );
  } else {
    const organisatieIds = useOrganisatieIds();
    const bronorganisatie = organisatieIds.value[0] || "";
    return await ensureKlantForBsn1(parameters, bronorganisatie);
  }
};
