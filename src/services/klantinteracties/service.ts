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
  BetrokkeneMetKlantContact as BetrokkeneWithKlantContact,
  ExpandedKlantContactApiViewmodel,
  ContactverzoekViewmodel,
  InternetaakApiViewModel,
} from "./types";
import { TypeOrganisatorischeEenheid } from "@/features/contact/components/types";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;
const klantinteractiesInterneTaken = `${klantinteractiesBaseUrl}/internetaken`;
const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;

////////////////////////////////////////////
// contactmomenten
function mapToContactmomentViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
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

function filterOutInternetaken(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<BetrokkeneWithKlantContact> {
  const filtered = value.page.filter(
    (item) => !item?.klantContact?.internetaak,
  );
  return {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: filtered,
  };
}

const fetchContactmomenten = async (
  url: string,
  gebruikKlantinteractiesApi: boolean,
) => {
  if (gebruikKlantinteractiesApi) {
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x as BetrokkeneWithKlantContact))
      .then(enrichBetrokkeneWithKlantContact)
      .then(enrichKlantcontactWithInterneTaak) //necesarry to filter them out
      .then(filterOutInternetaken)
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

        //dit is nodig of er moet een unique id prop meegegeven worden.
        //of er moet een unique id prop meegegeven worden
        //anders wordt alleen de CM's OF de CV's opgehaald
        //ze beginnen namelijk met dezelfde call, naar partij en als die hetzelfde is dan wordt die uit de cahce gehaald
        //maar dan wordt er blijkbaar geen promise geresolved, want dan wordt de rest van de .then(...) trein niet uitgevoerd
        //todo: SWRV eruit.als het al nutig is dan niet zo weggestopt in from fetcher
        searchParams.set("random", "1");

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

////////////////////////////////////////////
// contactmomenten and contactverzoeken
async function enrichBetrokkeneWithKlantContact(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
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
          betrokkene.klantContact = d
            .page[0] as ExpandedKlantContactApiViewmodel; // er is altijd maar 1 contact bij een betrokkeke!
        }
      });
  }
  return value;
}

////////////////////////////////////////////
// contactverzoeken
async function enrichKlantcontactWithInterneTaak(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    const searchParams = new URLSearchParams();
    searchParams.set(
      "klantcontact__uuid",
      betrokkeneWithKlantcontact.klantContact.uuid,
    );
    const url = `${klantinteractiesInterneTaken}?${searchParams.toString()}`;
    await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          betrokkeneWithKlantcontact.klantContact.internetaak = d
            .page[0] as InternetaakApiViewModel; //we mogen er vanuit gaan dat er 1 'hoofd interen tak' is bj een contact moment.
          // het model ondersteunt meerdere vervolg contacten, maar daar houden we binnen kiss nog geen rekening mee.
        }
      });
  }
  return value;
}

function filterOutContactmomenten(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<BetrokkeneWithKlantContact> {
  const filtered = value.page.filter((item) => item?.klantContact?.internetaak);
  return {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: filtered,
  };
}

function mapToContactverzoekViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<ContactverzoekViewmodel> {
  const viewmodel = value.page.map((x) => {
    return {
      url: x.klantContact.internetaak.url,
      record: {
        startAt: x.klantContact.internetaak.toegewezenOp,
        data: {
          status: x.klantContact.internetaak.status,
          contactmoment: x.klantContact.url, //todo: klopt dit???
          registratiedatum: x.klantContact.plaatsgevondenOp,
          datumVerwerkt: x.klantContact.internetaak.afgehandeldOp,
          toelichting: x.klantContact.internetaak.toelichting,
          actor: {
            naam: "",
            soortActor: "",
            identificatie: "",
            naamOrganisatorischeEenheid: "",
            typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling, //todo: ???
            identificatieOrganisatorischeEenheid: "",
          },

          betrokkene: {
            rol: "klant",
            //klant: null, //todo: wat moet hier, url, id ??
            persoonsnaam: {
              voornaam: "", //todo: partijgegevens
              voorvoegselAchternaam: "",
              achternaam: "",
            },
            organisatie: "",
            digitaleAdressen: [], //todo DigitaalAdres[];
          },
          verantwoordelijkeAfdeling: "", //todo: waa komt dit vandaan?
        },
      },
    } as ContactverzoekViewmodel;
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactverzoekViewmodel> =
    {
      next: value.next,
      previous: value.previous,
      count: value.count,
      page: viewmodel,
    };

  return paginatedContactenviewmodel;
}

export function useContactverzoekenByKlantIdApi(
  id: Ref<string>,
  gebruikKlantInteractiesApi: boolean,
) {
  function getUrl() {
    if (gebruikKlantInteractiesApi) {
      const searchParams = new URLSearchParams();
      searchParams.set("wasPartij__url", id.value);
      return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
    } else {
      if (!id.value) return "";
      const url = new URL("/api/internetaak/api/v2/objects", location.origin);
      url.searchParams.set("ordering", "-record__data__registratiedatum");
      url.searchParams.set("pageSize", "10");
      url.searchParams.set(
        "data_attrs",
        `betrokkene__klant__exact__${id.value}`,
      );
      return url.toString();
    }
  }

  const fetchContactverzoeken = (
    url: string,
    gebruikKlantinteractiesApi: boolean,
  ) => {
    if (gebruikKlantinteractiesApi) {
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((p) => parsePagination(p, (x) => x as BetrokkeneWithKlantContact))
        .then(enrichBetrokkeneWithKlantContact)
        .then(enrichKlantcontactWithInterneTaak)
        .then(filterOutContactmomenten)
        .then(mapToContactverzoekViewModel);
    } else {
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((r) => parsePagination(r, (v) => v as ContactverzoekViewmodel));
    }
  };

  return ServiceResult.fromFetcher(getUrl, (u: string) => {
    return fetchContactverzoeken(u, gebruikKlantInteractiesApi);
  });
}
