import { enforceOneOrZero, ServiceResult } from "@/services";
import { searchPersonen } from "@/services/brp";

export function usePersoonByBsn(getBsn: () => string | undefined) {
  const getCacheKey = () => {
    const bsn = getBsn();
    return bsn ? "persoon" + bsn : "";
  };
  const fetcher = (bsn: string) => {
    return searchPersonen({ bsn }).then(enforceOneOrZero);
  };
  return ServiceResult.fromFetcher(() => getBsn() || "", fetcher, {
    getUniqueId: getCacheKey,
  });
}
