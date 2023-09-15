import {
  ServiceResult,
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";
import { KlantType, type Klant } from "../types";

//const contactverzoekenBaseUrl = "/api/internetaak/api/v2/objects";

type SearchParameters = {
  // email?: string;
  // phone?: string;
  query: string;
};

//const klantRootUrl = new URL(document.location.href);
//klantRootUrl.pathname = contactverzoekenBaseUrl;

function getSearchUrl({ query = "" }: SearchParameters) {
  if (!query) return "";

  //const url = new URL(klantRootUrl);

  // url.searchParams.set("subjectType", KlantType.Persoon);

  // if (email) {
  //   url.searchParams.set("emailadres", email);
  // }

  // if (phone) {
  //   url.searchParams.set("telefoonnummer", phone);
  // }

  const url = new URL("/api/internetaak/api/v2/objects", location.href);
  url.searchParams.set("ordering", "-record__data__registratiedatum"); //todo: is dit de correcte orderning?
  url.searchParams.set("pageSize", "10");
  // url.searchParams.set("page", "1");
  url.searchParams.set(
    "data_attrs",
    `betrokkene__digitaleAdressen__icontains__${query}`,
  ); //todo: kan je in 1 call op email OF telefoonnr zoeken? anders de interface aanpassen zodat duidelijk is dat je maar naar een van beide kan zoeken (bv samenvoegen in 1 zoekveld!)

  return url.toString();
}

// // TODO: kijken of een gemeenschappelijke interface nog nodig is als het zoeken op contact/persoon/bedrijf uitgewerkt is
// function mapKlant(obj: any): Klant | null {
//   const { subjectIdentificatie, url, emailadres, telefoonnummer, klantnummer } =
//     obj ?? {};
//   const urlSplit: string[] = url?.split("/") ?? [];
//   const { inpBsn } = subjectIdentificatie ?? {};
//   // remove klanten with bsn
//   if (inpBsn) return null;

//   return {
//     id: urlSplit.at(-1) || "",
//     _typeOfKlant: "klant",
//     url: url,
//     emailadres,
//     telefoonnummer,
//     klantnummer,
//   };
// }

function searchRecursive(urlStr: string, page = 1): Promise<Klant[]> {
  //recursive alle pagina's ophalen.
  //set de page param van de huidige op te halen pagina
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return (
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      //todo: parsePagination toevoegen??
      .then((j) => {
        if (!Array.isArray(j?.results)) {
          throw new Error("expected array: " + JSON.stringify(j));
        }
        // const result: Klant[] = [];
        // j.results.forEach((k: any) => {
        //   const klant = mapKlant(k);
        //   if (klant) {
        //     result.push(klant);
        //     const idUrl = getKlantIdUrl(klant.id);
        //     if (idUrl) {
        //       mutate(idUrl, klant);
        //     }
        //   }
        // });

        const result: any[] = [];
        j.results.forEach((k: any) => {
          result.push(k);
        });

        if (!j.next) return result;
        return searchRecursive(urlStr, page + 1).then((next) => [
          ...result,
          ...next,
        ]);
      })
  );
}

// function compareNonEmpty(
//   a: string | null | undefined,
//   b: string | null | undefined,
// ) {
//   if (!a && !b) return 0;
//   if (!b) return -1;
//   if (!a) return 1;
//   return a.localeCompare(b);
// }

// function sortKlant(a: Klant, b: Klant) {
//   const compareEmail = compareNonEmpty(a.emailadres, b.emailadres);
//   if (compareEmail !== 0) return compareEmail;
//   return compareNonEmpty(a.telefoonnummer, b.telefoonnummer);
// }

function search(url: string) {
  return searchRecursive(url); //todo: sortering???  .then((r) => r.sort(sortKlant));
}

// function getKlantIdUrl(id?: string) {
//   if (!id) return "";
//   const url = new URL(`${klantRootUrl}/${id}`);
//   return url.toString();
// }

// export function useSearch(params: Ref<SearchParameters>) {
//   const getUrl = () => getSearchUrl(params.value);
//   return ServiceResult.fromFetcher(getUrl, search);
// }

export function useSearch(params: Ref<SearchParameters>) {
  const getUrl = () => getSearchUrl(params.value);
  return ServiceResult.fromFetcher(getUrl, search);
}
