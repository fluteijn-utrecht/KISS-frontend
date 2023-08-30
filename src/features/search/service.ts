import {
  parseJson,
  parseValidUrl,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
  type PaginatedResult,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";
import type { SearchResult, Source } from "./types";
import { computed } from "vue";

function mapResult(obj: any): SearchResult {
  const source = obj?._source?.object_bron ?? "Website";
  const id = obj?._id;

  const title = obj?._source?.headings?.[0] ?? obj?._source?.title;
  const content = obj?._source?.body_content;
  const url = parseValidUrl(obj?._source?.url);
  const documentUrl = new URL(location.href);
  // documentUrl.pathname = searchUrl;
  documentUrl.searchParams.set("query", id);

  const jsonObject = obj?._source?.[source] ?? null;
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

const pageSize = 10;

const getSearchUrl = (query: string, sources: Source[]) => {
  if (!query) return "";
  const uniqueIndices = [...new Set(sources.map((x) => x.index))];

  const url = new URL(location.href);
  url.pathname = `${globalSearchBaseUri}/${uniqueIndices
    .sort((a, b) => a.localeCompare(b))
    .join(",")}/_search`;

  return url.toString();
};

function mapSuggestions(json: any): string[] {
  if (!Array.isArray(json?.suggest?.suggestions)) return [];
  const result = [...json.suggest.suggestions].flatMap(({ options }: any) =>
    options.map(({ text }: any) => (text as string).toLocaleLowerCase()),
  ) as string[];
  return [...new Set(result)];
}

export function useGlobalSearch(
  parameters: Ref<{
    search?: string;
    page?: number;
    filters: Source[];
  }>,
) {
  const templateResult = useQueryTemplate();
  const template = computed(
    () => templateResult.success && templateResult.data.template,
  );

  const getUrl = () => {
    if (!template.value) return "";
    return getSearchUrl(
      parameters.value.search || "",
      parameters.value.filters,
    );
  };

  const getPayload = () => {
    if (!template.value || !parameters.value.search) return "";
    const page = parameters.value.page || 1;
    const from = (page - 1) * pageSize;

    const replaced = template.value.replace(
      /\{\{query\}\}/g,
      parameters.value.search,
    );

    const query = JSON.parse(replaced);
    query.from = from;
    query.size = pageSize;

    return JSON.stringify(query);
  };

  async function fetcher(
    url: string,
  ): Promise<Paginated<SearchResult> & { suggestions: string[] }> {
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
      suggestions: mapSuggestions(json),
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

const engineBaseUrl = "/api/enterprisesearch/api/as/v1/engines/kiss-engine";

function useQueryTemplate() {
  const url = engineBaseUrl + "/search_explain";

  const body = JSON.stringify({
    query: "{{query}}",
  });

  function fetcher(url: string) {
    return fetchLoggedIn(url, {
      method: "POST",
      body,
      headers: {
        "content-type": "application/json",
      },
    })
      .then(throwIfNotOk)
      .then(parseJson)
      .then(({ query_string, query_body }) => {
        delete query_body.from;
        delete query_body.size;
        delete query_body._source;
        query_body.indices_boost = [{ ".ent-search*": 1 }, { "*": 10 }];
        query_body.suggest = {
          suggestions: {
            prefix: "{{query}}",
            completion: {
              field: "_completion",
              skip_duplicates: true,
              fuzzy: {},
            },
          },
        };

        const searchUrl: string = query_string.split(" ").at(-1);
        const indicesStr = searchUrl.split("/")[0];
        const indices = indicesStr.split(",");
        return {
          indices,
          template: JSON.stringify(query_body, null, 2),
        };
      });
  }

  return ServiceResult.fromFetcher(url, fetcher);
}

const BRON_QUERY = JSON.stringify({
  size: 0,
  aggs: {
    bronnen: {
      terms: {
        field: "object_bron.enum",
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
  const templateResult = useQueryTemplate();
  const templateSources = computed(
    () =>
      templateResult.success &&
      !!templateResult.data.indices.length &&
      templateResult.data.indices,
  );

  const getUrl = () =>
    !templateSources.value
      ? ""
      : `${globalSearchBaseUri}/${templateSources.value.join(",")}/_search`;

  async function fetcher(u: string): Promise<Source[]> {
    const r = await fetchLoggedIn(u, {
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
        })),
    );

    return sources;
  }

  return ServiceResult.fromFetcher(getUrl, fetcher);
}

function useAfdelingenFieldNames() {
  const url = engineBaseUrl + "/schema";
  return ServiceResult.fromFetcher(url, (u) =>
    fetchLoggedIn(u)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((x) =>
        Object.keys(x).filter((x) => x.endsWith("afdelingen.afdelingNaam")),
      ),
  );
}

export function useArtikelAfdelingSearch(search: () => string | undefined) {
  const url = `${globalSearchBaseUri}/_search`;
  const fieldNames = useAfdelingenFieldNames();

  const getPayload = () => {
    if (!fieldNames.success || !fieldNames.data.length) return "";

    const enumFields = fieldNames.data.map((x) => `${x}.enum`);
    const searchFields = fieldNames.data.flatMap((x) => [
      x + "^1.0",
      x + ".delimiter^0.4",
      x + ".joined^0.75",
      x + ".prefix^0.1",
      x + ".stem^0.95",
    ]);

    const searchStr = search();

    const query = searchStr
      ? {
          query_string: {
            query: searchStr + "~",
            fields: searchFields,
          },
        }
      : undefined;

    const aggs = Object.fromEntries(
      enumFields.map((field, i) => [
        `agg${i}`,
        {
          terms: {
            field,
            size: 100,
          },
        },
      ]),
    );

    return JSON.stringify({
      aggs,
      query,
      size: 0,
    });
  };

  const fetcher = (body: string) =>
    fetchLoggedIn(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body,
    })
      .then(throwIfNotOk)
      .then(parseJson)
      .then((json) => {
        const set = new Set<string>();
        const result: string[] = [];
        const aggregations: any[] = Object.values(json.aggregations);
        aggregations.forEach(({ buckets }) => {
          (buckets as any[]).forEach(({ key }) => {
            const sanitized = key.trim().toLocaleLowerCase();
            if (!set.has(sanitized)) {
              set.add(sanitized);
              result.push(key);
            }
          });
        });
        result.sort();
        return result;
      });

  return ServiceResult.fromFetcher(getPayload, fetcher);
}
