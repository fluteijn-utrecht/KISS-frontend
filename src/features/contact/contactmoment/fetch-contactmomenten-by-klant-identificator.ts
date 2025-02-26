import { fetchContacmomentenByKlantUrlOk1 } from "@/services/contactmomenten/service";
import {
  type Systeem,
  registryVersions,
} from "@/services/environment/fetch-systemen";
import { fetchKlantByIdentifierOpenKlant1 } from "@/services/openklant1";
import {
  findKlantByIdentifier,
  fetchBetrokkenen,
  enrichBetrokkeneWithKlantContact,
  KlantContactExpand,
  mapKlantContactToContactmomentViewModel,
  type ContactmomentViewModel,
} from "@/services/openklant2";
import type { KlantIdentificator } from "../types";

export async function fetchContactmomentenByKlantIdentificator(
  id: KlantIdentificator,
  systemen: Systeem[],
): Promise<ContactmomentViewModel[]> {
  const defaultSysteem = systemen.find((x) => x.isDefault);
  if (!defaultSysteem) return [];

  const { bsn, vestigingsnummer, rsin, kvkNummer } = id;

  let klantIdentifier1, klantIdentifier2;

  switch (true) {
    case !!bsn:
      klantIdentifier1 = { bsn };
      klantIdentifier2 = { bsn };
      break;
    case !!vestigingsnummer:
      klantIdentifier1 = { vestigingsnummer };
      klantIdentifier2 = { vestigingsnummer };
      break;
    case !!kvkNummer:
      // esuite wil een kvkNummer als niet-natuurlijk-persoon-Id
      klantIdentifier1 = { nietNatuurlijkPersoonIdentifier: kvkNummer };
      klantIdentifier2 = { kvkNummer, rsin };
      break;
    case !!rsin:
      // dan maar proberen met de rsin?
      klantIdentifier1 = { nietNatuurlijkPersoonIdentifier: rsin };
      klantIdentifier2 = { rsin };
      break;

    default:
      return [];
  }

  const promises = systemen.map((systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      return fetchKlantByIdentifierOpenKlant1(
        systeem.identifier,
        klantIdentifier1,
      ).then((klant) =>
        !klant?.url
          ? []
          : fetchContacmomentenByKlantUrlOk1({
              systeemIdentifier: systeem.identifier,
              klantUrl: klant.url,
            }).then(({ page }) => page),
      );
    }

    return findKlantByIdentifier(systeem.identifier, klantIdentifier2).then(
      (klant) =>
        !klant?.url
          ? []
          : fetchBetrokkenen({
              systeemId: systeem.identifier,
              wasPartij__url: klant.url,
              pageSize: "100",
            })
              .then(async (paginated) =>
                enrichBetrokkeneWithKlantContact(
                  systeem.identifier,
                  paginated.page,
                  [KlantContactExpand.gingOverOnderwerpobjecten],
                ),
              )
              .then((page) =>
                page.map(({ klantContact }) =>
                  mapKlantContactToContactmomentViewModel(klantContact),
                ),
              ),
    );
  });

  return Promise.all(promises).then((all) =>
    all
      .flat()
      .sort(
        (a, b) =>
          new Date(b.registratiedatum).valueOf() -
          new Date(a.registratiedatum).valueOf(),
      ),
  );
}
