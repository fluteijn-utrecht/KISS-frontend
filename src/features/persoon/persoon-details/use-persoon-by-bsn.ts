import { enforceOneOrZero, ServiceResult } from "@/services";
import { searchPersonen } from "@/services/brp";
import type { Ref } from "vue";

export function usePersoonByBsn(
  getBsn: () => string | undefined,
  useOk2: Ref<boolean | null>,
) {
  // const getCacheKey = () => {
  //   if (useOk2.value === null) {
  //     return "";
  //   }
  //   const bsn = getBsn();
  //   return bsn ? "persoon" + bsn : "";
  // };
  const fetcher = (bsn: string) => {
    return searchPersonen({ bsn }).then(enforceOneOrZero);
  };
  return ServiceResult.fromFetcher(() => getBsn() || "", fetcher);
}
