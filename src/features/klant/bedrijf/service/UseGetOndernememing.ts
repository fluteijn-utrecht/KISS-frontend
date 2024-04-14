import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";
import type { BedrijfSearchParameter } from "../enricher/bedrijf-enricher";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByIdentifier = (
  getId: () => BedrijfSearchParameter | undefined,
) => {
  const getUrl = () => getUrlVoorGetBedrijfById(getId());
  console.log("=zoek bedrijf info=== getUrlVoorGetBedrijfById", getUrl());

  // useBedrijfByVestigingsnummer private //////////////////////////

  //regelt alleen maar een unieke id voor de cache ofzo. 
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
  console.log("=zoek bedrijf info=== getUrlVoorGetBedrijfById 1", ding);
  if (!ding || typeof ding != "object") {
    return "";
  }
  console.log("=zoek bedrijf info=== getUrlVoorGetBedrijfById 11", ding);
  const searchParams = new URLSearchParams();

  if ("vestigingsnummer" in ding) {
    if (!ding.vestigingsnummer) return "";
    searchParams.set("vestigingsnummer", ding.vestigingsnummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("kvkNummer" in ding) {
    if (!ding.kvkNummer) return "";
    searchParams.set("kvkNummer", ding.kvkNummer);
    return `${zoekenUrl}?${searchParams}`;
  } else if ("rsin" in ding) {
    searchParams.set("rsin", ding.rsin); //todo: is dit ok? is rsin en innnnpid hetzelfde? kvk api kent alleen rsin
    return `${zoekenUrl}?${searchParams}`;
  } else if ("innNnpId" in ding) {
    searchParams.set("rsin", ding.innNnpId); //todo: is dit ok? is rsin en innnnpid hetzelfde? kvk api kent alleen rsin
    return `${zoekenUrl}?${searchParams}`;
  }

  return "";
};
