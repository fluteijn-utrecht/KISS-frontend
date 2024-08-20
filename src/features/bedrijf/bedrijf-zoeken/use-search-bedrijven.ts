import { ServiceResult, type Paginated } from "@/services";

import {
  PartijTypes,
  searchKlantenByDigitaalAdres,
  type Klant,
} from "@/services/klanten";
import {
  searchBedrijvenInHandelsRegister,
  type Bedrijf,
  type BedrijfSearchOptions,
} from "@/services/kvk";

export function useSearchBedrijven(
  getArgs: () =>
    | BedrijfSearchOptions
    | { email: string }
    | { telefoonnummer: string }
    | undefined,
) {
  const getCacheKey = () => {
    const query = getArgs();
    return query ? "searchBedrijven" + JSON.stringify(query) : "";
  };

  const fetcher = () => {
    const query = getArgs();
    if (!query) {
      throw new Error("query wordt hierboven al gecheckt");
    }
    if ("email" in query || "telefoonnummer" in query)
      return searchKlantenByDigitaalAdres({
        ...query,
        partijType: PartijTypes.organisatie,
      });
    return searchBedrijvenInHandelsRegister(query);
  };
  return ServiceResult.fromFetcher<Klant[] | Paginated<Bedrijf>>(
    getCacheKey,
    fetcher,
  );
}
