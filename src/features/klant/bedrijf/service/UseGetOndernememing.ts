import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";
import type { BedrijfSearchParameter } from "../enricher/bedrijf-enricher";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByVestigingsnummer = (
  getId: () => BedrijfSearchParameter | undefined,
) => {
  const getUrl = () => getUrlVoorGetBedrijfById(getId());

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

const getUrlVoorGetBedrijfById = (ding: BedrijfSearchParameter | undefined) => {
  if (!ding) {
    return "";
  }

  const searchParams = new URLSearchParams();

  if ("vestigingsnummer" in ding) {
    if (!ding.vestigingsnummer) return "";
    searchParams.set("vestigingsnummer", ding.vestigingsnummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("kvkNummer" in ding) {
    if (!ding.kvkNummer) return "";
    searchParams.set("kvkNummer", ding.kvkNummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("innNnpId" in ding) {
    searchParams.set("rsin", ding.innNnpId); //todo: is dit ok? is rsin en innnnpid hetzelfde? kvk api kent alleen rsin
    return `${zoekenUrl}?${searchParams}`;
  }

  return "";
};
