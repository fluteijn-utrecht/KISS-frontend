import { 
    ServiceResult,
    fetchLoggedIn,
    parseJson,
    parsePagination,
    throwIfNotOk, 
    type PaginatedResult
} from "@/services";

export interface Groep {
    id: string;
    afdelingId: string;
    identificatie: string;
    naam: string;
  }

const getGroepenSearchUrl = (search: string | undefined, exactMatch: boolean) => {
const searchParams = new URLSearchParams();
searchParams.set("ordering", "record__data__naam");

const matchType = exactMatch ? "exact" : "icontains";
    if (search) {
        searchParams.set("data_attrs", `naam__${matchType}__${search.trim()}`);
    }
    return "/api/groepen/api/v2/objects?" + searchParams.toString();
};

const mapGroep = (x: any): Groep => ({
    id: x.uuid,
    afdelingId: x.afdelingId,
    naam: x.record.data.naam,
    identificatie: x.record.data.identificatie
});



const groepenFetcher = (url: string): Promise<PaginatedResult<Groep>> =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((json) => parsePagination(json, mapGroep));

export const fetchGroepen = (search: string, exactMatch: boolean) =>
    groepenFetcher(getGroepenSearchUrl(search, exactMatch));

export function useGroepen(search: () => string | undefined) {
    const getUrl = () => getGroepenSearchUrl(search(), false);
    return ServiceResult.fromFetcher(getUrl, groepenFetcher);
}
