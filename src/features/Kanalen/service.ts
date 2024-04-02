import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";

async function fetchAll(url: string) {
  return await fetchLoggedIn(url)
  .then(throwIfNotOk)
  .then((r) => r.json());
}



// const verwijderKanaal = (id: number) => {

//   return fetchLoggedIn("/api/KanaalVerwijderen/" + id, {
//     method: "DELETE",
//   })
//   .then(load)
// };


export function useKanalen() {
  const kanalen = ServiceResult.fromFetcher("/api/KanalenBeheerOverzicht", fetchAll, { poll: false } );

  return kanalen
}


export function useKanalenKeuzeLijst() {
  return ServiceResult.fromFetcher("/api/KanalenContactmomentKeuzelijst", fetchAll);
}