import { ServiceResult } from "@/services";
import { findKlantByIdentifier } from "../../service";
import type { BedrijfIdentifier } from "../types";

export const useKlantByBedrijfIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
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
  return ServiceResult.fromFetcher(getCacheKey, findKlant);
};
