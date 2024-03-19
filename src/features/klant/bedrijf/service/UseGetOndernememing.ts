import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByVestigingsnummer = (
  getVestigingsnummer: () => string | undefined,
) => {
  const getUrl = () => {
    const vestigingsnummer = getVestigingsnummer();
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
