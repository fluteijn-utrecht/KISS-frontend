import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
} from "@/services";
import type { Link } from "./types";

const linksUrl = (function () {
  return  "/api/links";
  
})();

function mapLink(json: any): Link {
  const { id, url, title, category } = json ?? {};
  return {
    id,
    url,
    title,
    category,
  };
}

function fetchLinks(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json())
  //  .then((json) => parsePagination(json, mapLink));
}

async function fetchAllLinks(url: string) {
    const first = await fetchLinks(url);

    return first;
}

export function useLinks() {
  return ServiceResult.fromFetcher(linksUrl, fetchAllLinks);
}
