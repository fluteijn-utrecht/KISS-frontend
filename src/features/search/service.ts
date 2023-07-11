import {
  parseJson,
  parseValidUrl,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";
import type { SearchResult, Source } from "./types";

function mapResult(obj: any): SearchResult {
  const source = obj?._index?.startsWith(".ent-search")
    ? "Website"
    : obj?._index ?? "";
  const id = obj?._id;

  const title = obj?._source?.headings?.[0] ?? obj?._source?.title;
  const content = obj?._source?.body_content;
  const url = parseValidUrl(obj?._source?.url);
  const documentUrl = new URL(location.href);
  // documentUrl.pathname = searchUrl;
  documentUrl.searchParams.set("query", id);

  const jsonObject = obj?._source?.object ?? null;
  return {
    source,
    id,
    title,
    content,
    url,
    jsonObject,
    documentUrl,
  };
}

const globalSearchBaseUri = "/api/elasticsearch";

const pageSize = 20;

export function useGlobalSearch(
  parameters: Ref<{
    search?: string;
    page?: number;
    filters: Source[];
  }>
) {
  const getUrl = () => {
    const uniqueIndices = [
      ...new Set(parameters.value.filters.map((x) => x.index)),
    ];

    const url = new URL(location.href);
    url.pathname = `${globalSearchBaseUri}/${uniqueIndices
      .sort((a, b) => a.localeCompare(b))
      .join(",")}/_search`;

    return url.toString();
  };

  const getPayload = () => {
    const page = parameters.value.page || 1;
    const from = (page - 1) * pageSize;

    return JSON.stringify({
      query: {
        simple_query_string: {
          query: parameters.value.search,
        },
      },
      from,
      size: pageSize,
    });
  };

  async function fetcher(url: string): Promise<Paginated<SearchResult>> {
    const r = await fetchLoggedIn(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body: getPayload(),
    });
    if (!r.ok) throw new Error();
    const json = await r.json();
    const {
      hits: { total, hits },
    } = json ?? {};
    const totalPages = Math.ceil((total?.value || 0) / pageSize);
    const page = Array.isArray(hits) ? hits.map(mapResult) : [];
    return {
      page,
      pageSize,
      pageNumber: parameters.value.page || 1,
      totalPages,
    };
  }

  function getUniqueId() {
    if (!parameters.value.search) return "";
    const payload = getPayload();
    const url = getUrl();
    return `${payload}${url}`;
  }

  return ServiceResult.fromFetcher(getUrl, fetcher, {
    getUniqueId,
  });
}

const BRON_QUERY = JSON.stringify({
  size: 0,
  aggs: {
    bronnen: {
      terms: {
        field: "object_bron.keyword",
      },
      aggs: {
        by_index: {
          terms: {
            field: "_index",
          },
        },
      },
    },
    domains: {
      terms: {
        field: "domains.enum",
      },
      aggs: {
        by_index: {
          terms: {
            field: "_index",
          },
        },
      },
    },
  },
});

export function useSources() {
  async function fetcher(): Promise<Source[]> {
    const r = await fetchLoggedIn(globalSearchBaseUri + "/_search", {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body: BRON_QUERY,
    });
    if (!r.ok) throw new Error();
    const json = await r.json();
    const {
      aggregations: { bronnen, domains },
    } = json ?? {};

    const sources: Source[] = [...bronnen.buckets, ...domains.buckets].flatMap(
      ({ key, by_index: { buckets } }) =>
        buckets.map((x: any) => ({
          index: x.key,
          name: key,
        }))
    );

    return sources;
  }

  return ServiceResult.fromFetcher(globalSearchBaseUri, fetcher, {
    getUniqueId: () => globalSearchBaseUri + "/sources",
  });
}

export function useSuggestions(input: Ref<string>) {
  function mapSuggestions(json: any): string[] {
    if (!Array.isArray(json?.results?.documents)) return [];
    return json.results.documents.map(({ suggestion }: any) => suggestion);
  }
  function fetchSuggestions() {
    return fetchLoggedIn("", {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body: JSON.stringify({
        query: input.value,
        size: 10,
        types: {
          documents: {
            fields: ["title", "body_content"],
          },
        },
      }),
    })
      .then(throwIfNotOk)
      .then(parseJson)
      .then(mapSuggestions);
  }
  function getUniqueId() {
    return input.value && "suggestions_" + input.value;
  }
  return ServiceResult.fromFetcher(() => "", fetchSuggestions, {
    getUniqueId,
  });
}
