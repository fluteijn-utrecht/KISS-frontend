import {
  fetchLoggedIn,
  parseJson,
  ServiceResult,
  throwIfNotOk,
} from "@/services";
import {
  DigitaalAdresTypes,
  fetchKlantById,
  klantinteractiesBaseUrl,
  PartijTypes,
} from "../../service";
import { mutate } from "swrv";

export function useSearchBedrijfKlantenByDigitaalAdres(
  getQuery: () =>
    | {
        telefoonnummer: string;
      }
    | {
        email: string;
      }
    | undefined,
) {
  function getUrl() {
    const query = getQuery();
    if (!query) return "";

    let key, value;

    if ("telefoonnummer" in query) {
      key = DigitaalAdresTypes.telefoonnummer;
      value = query.telefoonnummer;
    } else {
      key = DigitaalAdresTypes.email;
      value = query.email;
    }

    const searchParams = new URLSearchParams();
    searchParams.append(
      "verstrektDoorPartij__soortPartij",
      PartijTypes.organisatie,
    );
    searchParams.append("adres__icontains", value);

    searchParams.append("soortDigitaalAdres", key);

    return klantinteractiesBaseUrl + "/digitaleadressen?" + searchParams;
  }

  const searchKlantenViaDigitaleAdressen = (url: string) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then(
        async ({
          results,
        }: {
          results: { verstrektDoorPartij: { uuid: string } }[];
        }) => {
          const partijIds = results.map((x) => x.verstrektDoorPartij.uuid);
          const uniquePartijIds = [...new Set(partijIds)];
          const promises = uniquePartijIds.map(async (uuid) => {
            const klant = await fetchKlantById(uuid);
            // cache alvast de klant obv de id, zodat we deze al hebben als we naar de detailpagina navigeren
            mutate(klant.id, klant);
            return klant;
          });
          return Promise.all(promises);
        },
      );

  return ServiceResult.fromFetcher(getUrl, searchKlantenViaDigitaleAdressen);
}
