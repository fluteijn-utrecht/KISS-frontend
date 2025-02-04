import {
  findKlantByIdentifier,
  createKlant,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1 } from "@/services/openklant1/service";
import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user";
import { getRegisterDetails as getSysteemDetails } from "@/features/shared/systeemdetails";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";

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
      { bedrijfsnaam, identifier: mappedIdentifier },
      organisatieId,
    );
  }
};

// import {
//   findKlantByIdentifier,
//   createKlant,
//   type KlantBedrijfIdentifier,
// } from "@/services/openklant2";
// import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1 } from "@/services/openklant1/service";
// import { useOpenKlant2 } from "@/services/openklant2/service";
// import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
// import { useOrganisatieIds } from "@/stores/user";

// export const ensureKlantForBedrijfIdentifier = async (
//   identifier: KlantBedrijfIdentifier,
//   bedrijfsnaam: string,
// ) => {
//   const isOpenKlant2 = await useOpenKlant2();

//   if (isOpenKlant2) {
//     // Gebruik openklant2 implementatie
//     const klant = await findKlantByIdentifier(identifier);
//     return klant ?? (await createKlant(identifier));
//   } else {
//     // Gebruik openklant1 implementatie
//     const mappedIdentifier = mapBedrijfsIdentifier(identifier);
//     const organisatieIds = useOrganisatieIds();
//     const organisatieId = organisatieIds.value[0] || "";

//     return await ensureKlantForBedrijfIdentifierOk1(
//       {
//         bedrijfsnaam,
//         identifier: mappedIdentifier,
//       },
//       organisatieId,
//     );
//   }
// };
