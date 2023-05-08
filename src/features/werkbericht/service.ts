import type { Berichttype, Werkbericht } from "./types";
import {
  createLookupList,
  ServiceResult,
  type LookupList,
  type Paginated,
  type ServiceData,
} from "@/services";
import type { Ref } from "vue";
import { fetchLoggedIn } from "@/services";

const WP_MAX_ALLOWED_PAGE_SIZE = "100";
const BERICHTEN_BASE_URI = `${window.gatewayBaseUri}/api/kiss_openpub_pub`;

export type UseWerkberichtenParams = {
  type?: Berichttype;
  search?: string;
  skillIds?: number[];
  page?: number;
  pagesize?: number;
};

/**
 * Tries to parse a json object returned by the api as a Werkbericht
 * @param jsonObject a json object
 * @param getBerichtTypeNameById a function to get the name of a berichttype from it's id
 */
function parseWerkbericht({
  id,
  inhoud,
  isBelangrijk,
  datum,
  titel,
  type,
  skills,
  dateRead,
  url,
}: any = {}): Werkbericht {
  return {
    id,
    read: !!dateRead,
    title: titel,
    content: inhoud,
    date: datum && new Date(datum),
    type,
    skills,
    featured: isBelangrijk,
    url,
  };
}

/**
 * Fetches a lookuplist from the api
 * @param url
 */
function fetchLookupList(urlStr: string): Promise<LookupList<number, string>> {
  const url = new URL(urlStr);

  // having pagination here is a nuisance.
  if (!url.searchParams.has("page")) {
    url.searchParams.set("per_page", WP_MAX_ALLOWED_PAGE_SIZE);
  }

  return fetchLoggedIn(url)
    .then((r) => r.json())
    .then((json) => {
      if (!Array.isArray(json))
        throw new Error(
          "Invalide json, verwacht een lijst: " + JSON.stringify(json)
        );
      return json
        .filter((x) => typeof x?.id === "number" && typeof x?.slug === "string")
        .map((x) => [x.id, x.slug] as [number, string]);
    })
    .then(createLookupList);
}

/**
 * Returns a reactive ServiceData object promising a LookupList of berichttypes
 */
export function useBerichtTypes(): ServiceData<LookupList<number, string>> {
  const url = window.gatewayBaseUri + "/api/openpub/openpub_type";
  return ServiceResult.fromFetcher(url, fetchLookupList);
}

/**
 * Returns a reactive ServiceData object promising a LookupList of skills
 */
export function useSkills(): ServiceData<LookupList<number, string>> {
  const url = window.gatewayBaseUri + "/api/openpub/openpub_skill";
  return ServiceResult.fromFetcher(url, fetchLookupList);
}

/**
 * Returns a reactive ServiceData object promising a paginated list of Werkberichten.
 * This has a dependency on useBerichtTypes()
 * @param parameters
 */
export function useWerkberichten(
  parameters?: Ref<UseWerkberichtenParams>
): ServiceData<Paginated<Werkbericht>> {
  function getUrl() {
    const base = "/api/berichten/published";
    if (!parameters?.value) return base;

    const { type, search, page, skillIds } = parameters.value;

    const params: [string, string][] = [["pageSize", "10"]];

    if (type) {
      params.push(["type", type]);
    }

    if (search) {
      params.push(["search", search]);
    }

    if (page) {
      params.push(["page", page.toString()]);
    }

    if (skillIds?.length) {
      skillIds.forEach((skillId) => {
        params.push(["skillIds", skillId.toString()]);
      });
    }

    return `${base}?${new URLSearchParams(params)}`;
  }

  async function fetchBerichten(url: string): Promise<Paginated<Werkbericht>> {
    const r = await fetchLoggedIn(url);
    if (!r.ok) throw new Error(r.status.toString());

    const json: any[] = await r.json();

    const berichten = json.map(parseWerkbericht);

    if (!Array.isArray(berichten))
      throw new Error("expected a list, input: " + JSON.stringify(berichten));

    const featuredBerichten = berichten.filter(({ featured }) => featured);
    const regularBerichten = berichten.filter(({ featured }) => !featured);
    const sortedBerichten = [...featuredBerichten, ...regularBerichten];

    const intHeader = (name: string) => {
      const header = r.headers.get(name);
      if (!header) throw new Error("expected header with name " + name);
      return +header;
    };

    return {
      page: sortedBerichten,
      pageNumber: intHeader("X-Current-Page"),
      totalRecords: intHeader("X-Total-Records"),
      totalPages: intHeader("X-Total-Pages"),
      pageSize: intHeader("X-Page-Size"),
    };
  }

  return ServiceResult.fromFetcher(getUrl, fetchBerichten, { poll: true });
}

export function useFeaturedWerkberichtenCount() {
  async function fetchFeaturedWerkberichten(url: string): Promise<number> {
    const r = await fetchLoggedIn(url);

    if (!r.ok) throw new Error(r.status.toString());

    const json = await r.json();

    return json.count;
  }

  return ServiceResult.fromFetcher(
    "/api/berichten/featuredcount",
    fetchFeaturedWerkberichten,
    {
      poll: true,
    }
  );
}

export async function readBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}?fields[]`);

  if (!res.ok)
    throw new Error(`Expected to read bericht: ${res.status.toString()}`);

  return res.ok;
}

export async function unreadBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}`, {
    method: "PATCH",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ "@dateRead": false }),
  });

  if (!res.ok)
    throw new Error(`Expected to unread bericht: ${res.status.toString()}`);

  return res.ok;
}
