import { ServiceResult } from "@/services";
import {
  mapBedrijfsIdentifier,
  useKlantByIdentifier as useKlantByIdentifierOk1,
} from "@/services/openklant1/service";
import {
  findKlantByIdentifierOpenKlant2,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import {
  registryVersions,
  useSystemen,
} from "@/services/environment/fetch-systemen";

export const useKlantByBedrijfIdentifier = (
  getId: () => KlantBedrijfIdentifier | undefined,
) => {
  const getCacheKey = () => {
    const id = getId();
    if (!id) return "";
    return "klant" + JSON.stringify(id);
  };

  const { defaultSysteem } = useSystemen();

  const findKlant = async () => {
    const id = getId();
    if (!id) {
      throw new Error("Geen valide KlantBedrijfIdentifier");
    }

    if (defaultSysteem.value.registryVersion === registryVersions.ok2) {
      return findKlantByIdentifierOpenKlant2(
        defaultSysteem.value.identifier,
        id,
      );
    } else {
      const mappedId = mapBedrijfsIdentifier(id);
      return useKlantByIdentifierOk1(
        defaultSysteem.value.identifier,
        () => mappedId,
      );
    }
  };

  return ServiceResult.fromFetcher(getCacheKey(), findKlant, {
    getUniqueId: getCacheKey,
  });
};
