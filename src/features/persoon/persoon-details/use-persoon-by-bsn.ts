import { enforceOneOrZero, ServiceResult } from "@/services";
import { searchPersonen } from "@/services/brp";
import type { Ref } from "vue";

export function usePersoonByBsn(
  getBsn: () => string | undefined,
  useOk2: Ref<boolean | null>,
) {
  const getCacheKey = () => {
    console.log("brp getcachekey");

    if (useOk2.value === null) {
      return "";
    }

    console.log("brp getcachekey usok2");
    const bsn = getBsn();

    console.log("brp getcachekey bsn", bsn);

    return bsn ? "persoon" + bsn : "";
  };
  const fetcher = (bsn: string) => {
    console.log("brp gsearchpersonen");

    return searchPersonen({ bsn }).then(enforceOneOrZero);
  };
  return ServiceResult.fromFetcher(() => getBsn() || "", fetcher);
}
