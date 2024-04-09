import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";
import type { BedrijfSearchParameter } from "../enricher/bedrijf-enricher";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByVestigingsnummer = (
  getId: () => BedrijfSearchParameter | undefined,
) => {
  const getUrl = () => {
    const vestigingsnummer = getId();
    if (!vestigingsnummer) return "";
    const searchParams = new URLSearchParams();
    searchParams.set("vestigingsnummer", vestigingsnummer);
    return `${zoekenUrl}?${searchParams}`;
  };

  // useBedrijfByVestigingsnummer private //////////////////////////

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  const fetcher = (url: string) =>
    searchBedrijvenInHandelsRegister(url).then(enforceOneOrZero);

  return ServiceResult.fromFetcher(getUrl, fetcher, {
    getUniqueId,
  });
};
