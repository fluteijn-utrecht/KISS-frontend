import {
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  parsePagination,
} from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  filterOutContactmomenten,
  KlantContactExpand,
  mapToContactverzoekViewModel,
  searchDigitaleAdressen,
  type Betrokkene,
  type ContactverzoekViewmodel,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import { mapObjectToContactverzoekViewModel } from "@/services/openklant1";
import type { Contactverzoek } from "./types";

function searchRecursive(urlStr: string, page = 1): Promise<any[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (j) => {
      if (!Array.isArray(j?.results)) {
        throw new Error("Expected array: " + JSON.stringify(j));
      }

      if (!j.next) return j.results;
      const nextResults = await searchRecursive(urlStr, page + 1);
      return [...j.results, ...nextResults];
    });
}

async function searchOk2Recursive(
  adres: string,
  page = 1,
): Promise<DigitaalAdresExpandedApiViewModel[]> {
  const paginated = await searchDigitaleAdressen({
    adres,
    page,
    expand: [DigitaleAdressenExpand.verstrektDoorBetrokkene],
  });
  if (!paginated.next) return paginated.page;
  return [...paginated.page, ...(await searchOk2Recursive(adres, page + 1))];
}

export async function search(
  query: string,
  gebruikKlantInteractiesApi: boolean,
): Promise<Contactverzoek[]> {
  // OK2
  if (gebruikKlantInteractiesApi) {
    const adressen = await searchOk2Recursive(query);

    const betrokkenen = adressen
      .map((x) => x?._expand?.verstrektDoorBetrokkene)
      .filter(Boolean) as Betrokkene[];

    const uniqueBetrokkenen = new Map<string, Betrokkene>(
      betrokkenen.map((betrokkene) => [betrokkene.uuid, betrokkene]),
    );

    return (
      enrichBetrokkeneWithKlantContact(
        [...uniqueBetrokkenen.values()],
        [KlantContactExpand.leiddeTotInterneTaken],
      )
        .then(filterOutContactmomenten)
        .then(enrichBetrokkeneWithDigitaleAdressen)
        .then(enrichInterneTakenWithActoren)
        .then(mapToContactverzoekViewModel)
        // Filter voor OK2: alleen resultaten met 'wasPartij' null of undefined
        .then((r) => r.filter((x) => !x.record.data.betrokkene.wasPartij))
    );
  }
  /// OK1 heeft geen interne taak, dus gaan we naar de objecten registratie
  else {
    const url = new URL("/api/internetaak/api/v2/objects", location.origin);
    url.searchParams.set("ordering", "-record__data__registratiedatum");
    url.searchParams.set("pageSize", "10");
    url.searchParams.set(
      "data_attrs",
      `betrokkene__digitaleAdressen__icontains__${query}`,
    );

    const searchResults = await searchRecursive(url.toString());

    const mappedResults = searchResults.map(mapObjectToContactverzoekViewModel);

    // Filter voor OK1: alleen resultaten zonder 'klant'
    const filteredResults = mappedResults.filter(
      (item) => !item.record.data.betrokkene.klant,
    );

    return filteredResults;
  }
}

export function fetchContactverzoekenByKlantId(
  id: string,
  gebruikKlantInteractiesApi: boolean,
) {
  if (gebruikKlantInteractiesApi) {
    return fetchBetrokkenen({
      wasPartij__url: id,
    }).then(async (paginated) => ({
      ...paginated,
      page: await enrichBetrokkeneWithKlantContact(paginated.page, [
        KlantContactExpand.leiddeTotInterneTaken,
      ])
        .then(filterOutContactmomenten)
        .then(enrichBetrokkeneWithDigitaleAdressen)
        .then(enrichInterneTakenWithActoren)
        .then(mapToContactverzoekViewModel),
    }));
  }

  const url = new URL("/api/internetaak/api/v2/objects", location.origin);
  url.searchParams.set("ordering", "-record__data__registratiedatum");
  url.searchParams.set("pageSize", "10");
  url.searchParams.set("data_attrs", `betrokkene__klant__exact__${id}`);

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (v) => v as ContactverzoekViewmodel));
}
