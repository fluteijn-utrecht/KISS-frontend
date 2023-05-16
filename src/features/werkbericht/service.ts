import type { Berichttype, Werkbericht } from "./types";
import {
  createLookupList,
  ServiceResult,
  type LookupList,
  type Paginated,
  type ServiceData,
  throwIfNotOk,
  parseJson,
} from "@/services";
import type { Ref } from "vue";
import { fetchLoggedIn } from "@/services";

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
  isGelezen,
  url,
}: any = {}): Werkbericht {
  return {
    id,
    read: isGelezen,
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
 * Returns a reactive ServiceData object promising a LookupList of skills
 */
export function useSkills(): ServiceData<LookupList<number, string>> {
  const url = "/api/skills";
  const fetcher = (u: string) =>
    fetchLoggedIn(u)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((j: any[]) =>
        createLookupList(
          j.map(({ id, naam }: { id: number; naam: string }) => [id, naam])
        )
      );
  return ServiceResult.fromFetcher(url, fetcher);
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

    const json = await r.json();

    if (!Array.isArray(json))
      throw new Error("expected a list, input: " + JSON.stringify(page));

    const page = json.map(parseWerkbericht);

    const intHeader = (name: string) => {
      const header = r.headers.get(name);
      if (!header) throw new Error("expected header with name " + name);
      return +header;
    };

    return {
      page,
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

export async function putBerichtRead(id: string, isGelezen: boolean) {
  const res = await fetchLoggedIn(`/api/berichten/${id}/read`, {
    method: "PUT",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      isGelezen,
    }),
  });

  if (!res.ok)
    throw new Error(`Expected to read bericht: ${res.status.toString()}`);
}
