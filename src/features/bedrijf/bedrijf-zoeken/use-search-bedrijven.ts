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

export type BedrijvenQuery =
  | BedrijfSearchOptions
  | { email: string }
  | { telefoonnummer: string };

export function useSearchBedrijven(
  getArgs: () =>
    | {
        query: BedrijvenQuery;
        page: number;
      }
    | undefined,
) {
  const getCacheKey = () => {
    const args = getArgs();
    return args ? "searchBedrijven" + JSON.stringify(args) : "";
  };

  const fetcher = () => {
    const args = getArgs();
    if (!args) {
      throw new Error("query wordt hierboven al gecheckt");
    }

    const { page, query } = args;

    if ("email" in query || "telefoonnummer" in query)
      return searchKlantenByDigitaalAdres({
        ...query,
        partijType: PartijTypes.organisatie,
      }).then((r) => ({
        page: r,
      }));

    return searchBedrijvenInHandelsRegister(query, page);
  };
  return ServiceResult.fromFetcher<{ page: Klant[] } | Paginated<Bedrijf>>(
    getCacheKey,
    fetcher,
  );
}
