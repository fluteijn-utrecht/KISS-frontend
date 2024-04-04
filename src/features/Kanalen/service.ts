import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";

async function fetchAll(url: string) {
  return await fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json());
}

export function useKanalen() {
  const kanalen = ServiceResult.fromFetcher(
    "/api/KanalenBeheerOverzicht",
    fetchAll,
  );

  return kanalen;
}

export function useKanalenKeuzeLijst() {
  return ServiceResult.fromFetcher(
    "/api/KanalenContactmomentKeuzelijst",
    fetchAll,
  );
}

export function verwijderkanaal(id: number) {
  return fetchLoggedIn("/api/KanaalVerwijderen/" + id, {
    method: "DELETE",
  });
}
