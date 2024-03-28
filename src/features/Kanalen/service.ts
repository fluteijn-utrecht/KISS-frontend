import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";


async function fetchAll(url: string) {
  return await fetchLoggedIn(url)
  .then(throwIfNotOk)
  .then((r) => r.json());
}

export function useKanalen() {
  return ServiceResult.fromFetcher("/api/KanalenBeheerOverzicht", fetchAll);
}
