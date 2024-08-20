import { enforceOneOrZero, ServiceResult } from "@/services";
import { searchPersonen } from "@/services/brp";

export function usePersoonByBsn(getBsn: () => string | undefined) {
  const getCacheKey = () => {
    const bsn = getBsn() || "";
    return bsn && "persoon" + bsn;
  };
  return ServiceResult.fromFetcher(getCacheKey, (bsn) =>
    searchPersonen({ bsn }).then(enforceOneOrZero),
  );
}
