import { ServiceResult, enforceOneOrZero } from "@/services";
import { searchBedrijvenInHandelsRegister } from "./shared/shared";
import type { BedrijfIdentifier } from "../types";

const zoekenUrl = "/api/kvk/v2/zoeken";

export const useBedrijfByIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
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
  bedrijfsZoekParamter: BedrijfIdentifier | undefined,
) => {
  if (!bedrijfsZoekParamter || typeof bedrijfsZoekParamter != "object") {
    return "";
  }

  const searchParams = new URLSearchParams();

  if (
    "vestigingsnummer" in bedrijfsZoekParamter &&
    bedrijfsZoekParamter.vestigingsnummer
  ) {
    searchParams.set("vestigingsnummer", bedrijfsZoekParamter.vestigingsnummer);
    return `${zoekenUrl}?${searchParams}`;
  }

  if (
    "nietNatuurlijkPersoonIdentifier" in bedrijfsZoekParamter &&
    bedrijfsZoekParamter.nietNatuurlijkPersoonIdentifier
  ) {
    const identifier =
      bedrijfsZoekParamter.nietNatuurlijkPersoonIdentifier.length === 9
        ? "rsin"
        : "kvkNummer";
    searchParams.set(
      identifier,
      bedrijfsZoekParamter.nietNatuurlijkPersoonIdentifier,
    );
    searchParams.set("type", "rechtspersoon");
    return `${zoekenUrl}?${searchParams}`;
  }

  return "";
};
