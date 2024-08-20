import { ServiceResult, enforceOneOrZero } from "@/services";
import {
  searchBedrijvenInHandelsRegister,
  type BedrijfIdentifier,
} from "@/services/kvk";

export const useBedrijfByIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
) => {
  const getCacheKey = () => {
    const id = getId();
    if (!id) return "";
    const identfier = "vestigingsnummer" in id ? id.vestigingsnummer : id.rsin;
    return "bedrijf" + identfier;
  };

  const fetcher = () => {
    const id = getId();
    if (!id) {
      throw new Error("Dit wordt hierboven al afgehandeld");
    }
    return searchBedrijvenInHandelsRegister(id).then(enforceOneOrZero);
  };

  return ServiceResult.fromFetcher(getCacheKey, fetcher);
};
