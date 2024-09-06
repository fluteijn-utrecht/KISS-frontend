import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type { Ref } from "vue";
import type {
  ContactmomentViewModel,
  BetrokkeneMetKlantContact,
  ExpandedKlantContactViewmodel,
} from "./types";
import type { KlantContactPostmodel } from "@/features/contact/contactmoment/types";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;
const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;

function mapToContactmomentViewModel(
  value: PaginatedResult<BetrokkeneMetKlantContact>,
) {
  const viewmodel = value.page.map((x) => {
    const medewerker = x.klantContact?.hadBetrokkenActoren?.find(
      (x: { soortActor: string }) => x.soortActor === "medewerker",
    );
    return {
      url: x.klantContact.url,
      registratiedatum: x.klantContact?.plaatsgevondenOp,
      kanaal: x.klantContact?.kanaal,
      tekst: x.klantContact?.inhoud,
      objectcontactmomenten: [], //wordt uitgesteld. besproken in https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/800
      medewerkerIdentificatie: {
        identificatie: medewerker?.actorIdentificator?.objectId || "",
        voorletters: "",
        achternaam: medewerker?.naam || "",
        voorvoegselAchternaam: "",
      },
    };
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactmomentViewModel> = {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: viewmodel,
  };

  return paginatedContactenviewmodel;
}

async function enrichBetrokkeneWithKlantContact(
  value: PaginatedResult<BetrokkeneMetKlantContact>,
): Promise<PaginatedResult<BetrokkeneMetKlantContact>> {
  for (const betrokkene of value.page) {
    const searchParams = new URLSearchParams();
    searchParams.set("hadBetrokkene__uuid", betrokkene.uuid);
    const url = `${klantinteractiesKlantcontacten}?${searchParams.toString()}`;
    await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          betrokkene.klantContact = d.page[0] as ExpandedKlantContactViewmodel; // er is altijd maar 1 contact bij een betrokkeke!
        }
      });
  }
  return value;
}

const fetchContactmomenten = async (
  url: string,
  gebruikKlantinteractiesApi: boolean,
) => {
  if (gebruikKlantinteractiesApi) {
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x as BetrokkeneMetKlantContact))
      .then(enrichBetrokkeneWithKlantContact)
      .then(mapToContactmomentViewModel);
  } else {
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));
  }
};

export function useContactmomentenByKlantIdApi(
  id: Ref<string>,
  gebruikKlantinteractiesApi: boolean,
) {
  return ServiceResult.fromFetcher(
    () => {
      // retourneer een url voor openklant 1 OF de klantInteracties api
      if (gebruikKlantinteractiesApi) {
        const searchParams = new URLSearchParams();
        searchParams.set("wasPartij__url", id.value);
        return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
      } else {
        if (!id.value) return "";
        const searchParams = new URLSearchParams();
        searchParams.set("klant", id.value);
        searchParams.set("ordering", "-registratiedatum");
        searchParams.set("expand", "objectcontactmomenten");
        return `${contactmomentenUrl}?${searchParams.toString()}`;
      }
    },
    (u: string) => fetchContactmomenten(u, gebruikKlantinteractiesApi),
  );
}
