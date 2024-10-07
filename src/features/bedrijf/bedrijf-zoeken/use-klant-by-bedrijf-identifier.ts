import { ServiceResult } from "@/services";
import {
  findKlantByIdentifier,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";

export const useKlantByBedrijfIdentifier = (
  getId: () => KlantBedrijfIdentifier | undefined,
) => {
  const getCacheKey = () => {
    const id = getId();
    if (!id) return "";
    return "klant" + JSON.stringify(getId());
  };
  const findKlant = () => {
    const id = getId();
    if (!id) {
      throw new Error();
    }
    return findKlantByIdentifier(id);
  };
  return ServiceResult.fromFetcher("", findKlant, {
    getUniqueId: getCacheKey,
  });
};
