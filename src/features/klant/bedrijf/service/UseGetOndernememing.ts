import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";
import type { BedrijfSearchParameter } from "../enricher/bedrijf-enricher";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByIdentifier = (
  getId: () => BedrijfSearchParameter | undefined,
) => {
  const getUrl = () => getUrlVoorGetBedrijfById(getId());

  // useBedrijfByVestigingsnummer private //////////////////////////

  //regelt alleen maar een unieke id voor de cache.
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

const getUrlVoorGetBedrijfById = (
  bedrijfsZoekParamter: BedrijfSearchParameter | undefined,
) => {
  if (!bedrijfsZoekParamter || typeof bedrijfsZoekParamter != "object") {
    return "";
  }

  const searchParams = new URLSearchParams();

  if ("vestigingsnummer" in bedrijfsZoekParamter) {
    if (!bedrijfsZoekParamter.vestigingsnummer) return "";
    searchParams.set("vestigingsnummer", bedrijfsZoekParamter.vestigingsnummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("kvkNummer" in bedrijfsZoekParamter) {
    if (!bedrijfsZoekParamter.kvkNummer) return "";
    searchParams.set("kvkNummer", bedrijfsZoekParamter.kvkNummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("rsin" in bedrijfsZoekParamter) {
    searchParams.set("rsin", bedrijfsZoekParamter.rsin);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("innNnpId" in bedrijfsZoekParamter) {
    searchParams.set("rsin", bedrijfsZoekParamter.innNnpId);
    return `${zoekenUrl}?${searchParams}`;
  }

  return "";
};
