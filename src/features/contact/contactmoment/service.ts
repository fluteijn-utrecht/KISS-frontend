import {
  throwIfNotOk,
  ServiceResult,
  parsePagination,
  parseJson,
  type PaginatedResult,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";

import {
  type Gespreksresultaat,
  type ContactmomentObject,
  type Contactmoment,
  type ZaakContactmoment,
  type ObjectContactmoment,
  type ContactmomentDetails,
  type SaveContactmomentResponseModel,
  type KlantContact,
  type SaveKlantContactResponseModel,
} from "./types";
import type { ContactmomentViewModel } from "@/features/shared/types";
import { toRelativeProxyUrl } from "@/helpers/url";

import { formatIsoDate } from "@/helpers/date";
import {
  ActorType,
  type ContactmomentContactVerzoek,
} from "@/stores/contactmoment";

import {
  TypeOrganisatorischeEenheid,
  type Vraag,
  type ContactverzoekData,
  type DigitaalAdres,
  type NewContactverzoek,
} from "../components/types";
import {
  isCheckboxVraag,
  isDropdownVraag,
  isInputVraag,
  isTextareaVraag,
} from "../components/service";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentDetails = "/api/contactmomentdetails";
const objectcontactmomentenUrl = `${contactmomentenBaseUrl}/objectcontactmomenten`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;
const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export const saveContactmoment = async (
  data: Contactmoment,
): Promise<SaveContactmomentResponseModel> => {
  const response = await postContactmoment(data);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postContactmoment = (data: Contactmoment): Promise<Response> => {
  return fetchLoggedIn(`/api/postcontactmomenten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const saveKlantContact = async (
  data: KlantContact,
): Promise<SaveKlantContactResponseModel> => {
  const response = await postKlantContact(data);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postKlantContact = (data: KlantContact): Promise<Response> => {
  return fetchLoggedIn(`/api/postklantcontacten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const CONTACTVERZOEK_GEMAAKT = "Contactverzoek gemaakt";

export const useGespreksResultaten = () => {
  const fetchBerichten = (url: string) =>
    fetchLoggedIn(url)
      .then((r) => {
        if (!r.ok) {
          throw new Error(
            "Er is een fout opgetreden bij het laden van de gespreksresultaten",
          );
        }
        return r.json();
      })
      .then((results: Array<Gespreksresultaat>) => {
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(results));
        const hasContactverzoekResultaat = results.some(
          ({ definitie }) => definitie === CONTACTVERZOEK_GEMAAKT,
        );
        if (!hasContactverzoekResultaat) {
          results.push({
            definitie: CONTACTVERZOEK_GEMAAKT,
          });
          results.sort((a, b) => a.definitie.localeCompare(b.definitie));
        }
        return results;
      });

  return ServiceResult.fromFetcher("/api/gespreksresultaten", fetchBerichten);
};

export const koppelObject = (data: ContactmomentObject) =>
  fetchLoggedIn(objectcontactmomentenUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  }).then(throwIfNotOk);

export const koppelZaakContactmoment = ({
  zaaksysteemId,
  url,
  contactmoment,
}: ZaakContactmoment) =>
  fetchLoggedIn(zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ zaaksysteemId, url, contactmoment }),
  }).then(throwIfNotOk);

export function koppelKlant({
  klantId,
  contactmomentId,
}: {
  klantId: string;
  contactmomentId: string;
}) {
  return fetchLoggedIn(klantcontactmomentenUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      klant: klantId,
      contactmoment: contactmomentId,
      rol: "gesprekspartner",
    }),
  }).then(throwIfNotOk) as Promise<void>;
}

export function koppelBetrokkenen({
  partijId,
  contactmomentId,
}: {
  partijId: string;
  contactmomentId: string;
}) {
  return fetchLoggedIn(klantinteractiesBetrokkenen, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      wasPartij: {
        uuid: partijId,
      },
      hadKlantcontact: {
        uuid: contactmomentId,
      },
      rol: "klant",
      initiator: true,
    }),
  }).then(throwIfNotOk) as Promise<void>;
}

type Ok2BetrokkeneMetKlantContacten = {
  uuid: string;

  klantContact: KlantContact;
};

function enrichBetrokkeneWithKlantContact(
  value: PaginatedResult<Ok2BetrokkeneMetKlantContacten>,
): PaginatedResult<any> | PromiseLike<PaginatedResult<any>> {
  value.page.forEach(async (betrokkene) => {
    const searchParams = new URLSearchParams();
    searchParams.set("hadBetrokkene__uuid", betrokkene.uuid);
    const url = `${klantinteractiesKlantcontacten}?${searchParams.toString()}`;

    ServiceResult.fromPromise(
      fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((p) => parsePagination(p, (x) => x as KlantContact))
        .then((d) => (betrokkene.klantContact = d.page[0])), // er is altijd maar 1 contact bij een betrokkeke! //todo check of [0] bestaat
    );
  });

  return value;
}

function mapToContactmomentViewModel(
  value: PaginatedResult<Ok2BetrokkeneMetKlantContacten>,
) {
  console.log(123123123, value);
  const viewmodel = value.page.map((x) => {
    const contactmomentViewModel: ContactmomentViewModel = {
      url: "", //todo
      registratiedatum: x.klantContact.plaatsgevondenOp,
      medewerker: "", //x.klantContact. .... //todo medewerker opzoeken,
      kanaal: x.klantContact.kanaal,
      tekst: x.klantContact.inhoud,
      objectcontactmomenten: [], //todo vullen (of is dit een aparte story)
      medewerkerIdentificatie: {
        identificatie: "",
        voorletters: "",
        achternaam: "",
        voorvoegselAchternaam: "",
      }, //todo opzoeken en wat is het verschil met het veld 'medewerker' ???
    };

    return contactmomentViewModel;
  });

  return viewmodel;
}

function GetOk2Contactmomenten(partijId: string) {
  const contactmomentenPage = ServiceResult.fromFetcher(
    () => {
      const searchParams = new URLSearchParams();
      searchParams.set("wasPartij__url", partijId);
      return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
    },
    (url: string) =>
      fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((p) =>
          parsePagination(p, (x) => {
            const v: ContactmomentViewModel = {
              url: "",
              registratiedatum: "",
              medewerker: "",
              kanaal: "",
              tekst: "",
              objectcontactmomenten: [],
              medewerkerIdentificatie: {
                identificatie: "",
                achternaam: "",
                voorletters: "",
                voorvoegselAchternaam: "",
              },
            };

            return v;
          }),
        ),
    //.then(enrichBetrokkeneWithKlantContact)
    //.then(mapToContactmomentViewModel),
  );

  //const searchParams = new URLSearchParams();
  //searchParams.set("wasPartij__url", partijId);
  //// searchParams.set("page", String(page));
  // const url = `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;

  // const contactmomentenPage = ServiceResult.fromPromise(
  //   //(u: string) => {

  //   fetchLoggedIn(url)
  //     .then(throwIfNotOk)
  //     .then(parseJson)
  //     .then((p) =>
  //       parsePagination(p, (x) => x as Ok2BetrokkeneMetKlantContacten),
  //     )
  //     .then(enrichBetrokkeneWithKlantContact)
  //     .then(mapToContactmomentViewModel),
  //   // },
  // );

  //we gingen bij ok 1 uit van een enkele result page?
  //blijft dat zo?

  return contactmomentenPage;
}

const fetchOk1Contactmomenten = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));

const fetchOk1OfOk2Contactmomenten = (u: string) => {
  return fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));
};

// function GetOk2Contactmomenten(id: string) {
//   //haal betrokkeken op obv partij uuid
//   //haal van alle betrokkeken de klantcontacten op obv betrokkeken url of id.

//   return GetOk2betrokkenenBypartijId(id, 1);
// }

// function getContactmomentenFromAppropriateSource(
//   value: boolean,
// ): boolean | PromiseLike<boolean> {
//   if (value) {
//     return GetOk2Contactmomenten(id.value);
//   } else {
//     return ServiceResult.fromFetcher(() => {
//       if (!id.value) return "";
//       const searchParams = new URLSearchParams();
//       searchParams.set("klant", id.value);
//       searchParams.set("ordering", "-registratiedatum");
//       searchParams.set("expand", "objectcontactmomenten");
//       return `${contactmomentenUrl}?${searchParams.toString()}`;
//     }, fetchOk1Contactmomenten);
//   }
// }

export function useContactmomentenByKlantId(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => {
      //retourneer de url van ok1 OF ok2
      if (!id.value) return "";
      const searchParams = new URLSearchParams();
      searchParams.set("klant", id.value);
      searchParams.set("ordering", "-registratiedatum");
      searchParams.set("expand", "objectcontactmomenten");
      return `${contactmomentenUrl}?${searchParams.toString()}`;
    },
    (u: string) => fetchOk1OfOk2Contactmomenten(u),
  );
}

//   () => {

//   //  const x = await isOk2DefaultContactenApi()

//       return   fetchOk1Contactmomenten;
// )}

// if (useKlantInteractiesApi) {
//   return GetOk2Contactmomenten(id.value);
// } else {
//   return ServiceResult.fromFetcher(() => {
//     if (!id.value) return "";
//     const searchParams = new URLSearchParams();
//     searchParams.set("klant", id.value);
//     searchParams.set("ordering", "-registratiedatum");
//     searchParams.set("expand", "objectcontactmomenten");
//     return `${contactmomentenUrl}?${searchParams.toString()}`;
//   }, fetchOk1Contactmomenten);
// }
//}

export const useContactmomentDetails = (url: () => string) =>
  ServiceResult.fromFetcher(
    () => {
      const u = url();
      if (!u) return "";
      const searchParams = new URLSearchParams();
      searchParams.set("id", u);
      return `${contactmomentDetails}?${searchParams.toString()}`;
    },
    (url) =>
      fetchLoggedIn(url).then((r) => {
        if (r.status === 404) return null;
        throwIfNotOk(r);
        return r.json() as Promise<ContactmomentDetails>;
      }),
  );

export function useContactmomentenByObjectUrl(url: Ref<string>) {
  const getUrl = () => {
    if (!url.value) return "";
    const params = new URLSearchParams();
    params.set("object", url.value);
    params.set("ordering", "-registratiedatum");
    params.set("expand", "objectcontactmomenten");
    return `${contactmomentenUrl}?${params}`;
  };

  return ServiceResult.fromFetcher(getUrl, fetchOk1Contactmomenten);
}

export function useContactmomentObject(getUrl: () => string) {
  return ServiceResult.fromFetcher(
    () => {
      const u = getUrl();
      if (!u) return "";
      return toRelativeProxyUrl(u, contactmomentenProxyRoot) || "";
    },
    (u) =>
      fetchLoggedIn(u)
        .then(throwIfNotOk)
        .then(parseJson) as Promise<ObjectContactmoment>,
  );
}

export function useContactmomentByUrl(getUrl: () => string) {
  return ServiceResult.fromFetcher(
    () => {
      const u = getUrl();
      if (!u) return "";
      const url = toRelativeProxyUrl(u, contactmomentenProxyRoot);
      if (!url) return "";
      const params = new URLSearchParams({
        expand: "objectcontactmomenten",
      });
      return `${url}?${params}`;
    },
    (u) =>
      fetchLoggedIn(u).then((r) => {
        if (r.status === 404) return null;
        throwIfNotOk(r);
        return r.json() as Promise<ContactmomentViewModel>;
      }),
  );
}

export function saveContactverzoek({
  data,
  contactmomentUrl,
}: {
  data: Omit<ContactverzoekData, "contactmoment">;
  contactmomentUrl: string;
  klantUrl?: string;
}) {
  const url = "/api/internetaak/api/v2/objects";

  const body: NewContactverzoek = {
    record: {
      // typeVersion wordt nu geconfigureerd en ingesteld in de backend.
      // Dit vereenvoudigt de frontend code en centraliseert de configuratie.
      startAt: formatIsoDate(data.registratiedatum),
      data: {
        ...data,
        contactmoment: contactmomentUrl,
      },
    },
  };

  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  })
    .then(throwIfNotOk)
    .then((r) => r.json() as Promise<{ url: string }>);
}

export function mapContactverzoekData({
  data,
  klantUrl,
}: {
  data: ContactmomentContactVerzoek;
  klantUrl?: string;
}): Omit<ContactverzoekData, "contactmoment"> {
  const now = new Date();
  const registratiedatum = now.toISOString();
  const digitaleAdressen = [] as DigitaalAdres[];
  if (data.emailadres) {
    digitaleAdressen.push({
      adres: data.emailadres,
      omschrijving: "e-mailadres",
      soortDigitaalAdres: "e-mailadres",
    });
  }
  if (data.telefoonnummer1) {
    digitaleAdressen.push({
      adres: data.telefoonnummer1,
      omschrijving: "telefoonnummer",
      soortDigitaalAdres: "telefoonnummer",
    });
  }
  if (data.telefoonnummer2) {
    digitaleAdressen.push({
      adres: data.telefoonnummer2,
      omschrijving: data.omschrijvingTelefoonnummer2 || "telefoonnummer",
      soortDigitaalAdres: "telefoonnummer",
    });
  }

  function formatVraagAntwoordForToelichting(vraagAntwoord: Vraag[]): string {
    return vraagAntwoord
      .map((va) => {
        if (isInputVraag(va)) {
          return `${va.description}: ${va.input}`;
        } else if (isTextareaVraag(va)) {
          return `${va.description}: ${va.textarea}`;
        } else if (isDropdownVraag(va)) {
          return `${va.description}: ${va.selectedDropdown}`;
        } else if (isCheckboxVraag(va)) {
          const selectedOptions = va.options
            .filter((_, index) => va.selectedCheckbox[index])
            .join(", ");
          return `${va.description}: ${selectedOptions}`;
        }
        return null;
      })
      .filter(Boolean)
      .join("\n");
  }

  const vragenToelichting =
    data.contactVerzoekVragenSet &&
    data.contactVerzoekVragenSet.vraagAntwoord &&
    data.contactVerzoekVragenSet.vraagAntwoord.length
      ? formatVraagAntwoordForToelichting(
          data.contactVerzoekVragenSet.vraagAntwoord,
        )
      : "";

  let verantwoordelijkheAfdeling = "";
  if (data.groep) {
    verantwoordelijkheAfdeling = data.groep.naam;
  } else if (data.afdeling) {
    verantwoordelijkheAfdeling = data.afdeling.naam;
  } else if (data.organisatorischeEenheidVanMedewerker) {
    verantwoordelijkheAfdeling =
      data.organisatorischeEenheidVanMedewerker.naam.split(": ")[1] || "";
  }

  // groep
  let actor = null;

  if (data.typeActor == ActorType.groep) {
    if (data.medewerker) {
      //voor een medewerker van een groep
      actor = {
        naam: data.medewerker.achternaam || "",
        soortActor: "medewerker",
        identificatie: data.medewerker?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Groep,
        naamOrganisatorischeEenheid: data.groep?.naam || "",
        identificatieOrganisatorischeEenheid: data.groep?.identificatie || "",
      };
    } else {
      //alleen een groep. geen medewerker geselecteerd
      actor = {
        naam: data.groep?.naam || "",
        soortActor: "organisatorische eenheid",
        identificatie: data.groep?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Groep,
      };
    }
  }
  if (data.typeActor == ActorType.afdeling) {
    if (data.medewerker) {
      //voor een medewerker van een afdeling
      actor = {
        naam: data.medewerker.achternaam || "",
        soortActor: "medewerker",
        identificatie: data.medewerker?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling,
        naamOrganisatorischeEenheid: data.afdeling?.naam || "",
        identificatieOrganisatorischeEenheid:
          data.afdeling?.identificatie || "",
      };
    } else {
      //alleen een afdeling. geen medewerker geselecteerd
      actor = {
        naam: data.afdeling?.naam || "",
        soortActor: "organisatorische eenheid",
        identificatie: data.afdeling?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling,
      };
    }
  }
  if (data.typeActor == ActorType.medewerker) {
    actor = {
      naam: data.medewerker?.achternaam || "",
      soortActor: "medewerker",
      identificatie: data.medewerker?.identificatie || "",
      typeOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.naam
          ?.split(": ")[0]
          ?.toLowerCase() === "afdeling"
          ? TypeOrganisatorischeEenheid.Afdeling
          : TypeOrganisatorischeEenheid.Groep,
      naamOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.naam.split(": ")[1] || "",
      identificatieOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.identificatie || "",
    };
  }

  if (actor === null) {
    throw new Error("actor kan niet bepaald worden");
  }

  return {
    verantwoordelijkeAfdeling: verantwoordelijkheAfdeling,
    status: "te verwerken",
    registratiedatum,
    toelichting:
      data.interneToelichting +
      (vragenToelichting ? "\n\n" + vragenToelichting : ""),
    actor: actor,
    betrokkene: {
      rol: "klant",
      klant: klantUrl,
      persoonsnaam: {
        voornaam: data.voornaam,
        voorvoegselAchternaam: data.voorvoegselAchternaam,
        achternaam: data.achternaam,
      },
      organisatie: data.organisatie,
      digitaleAdressen,
    },
  };
}

export async function isOk2DefaultContactenApi() {
  return false;
  // bepaal of de openklant api of de klantinteracties api gebruikt moet worden voor verwerken van contactmomenten en contactverzoeken
  // Fetch USE_KLANTCONTACTEN environment variable, wordt in sommige gevallen vervangen door flow te bepalen op basis van zaken
  const response = await fetch("/api/environment/use-klantcontacten");
  const { useKlantContacten } = await response.json();
  return useKlantContacten as boolean;
}
