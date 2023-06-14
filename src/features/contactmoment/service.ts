import { formatDateOnly, formatTimeOnly } from "@/helpers/date";
import {
  throwIfNotOk,
  ServiceResult,
  type Paginated,
  parsePagination,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";

import type {
  Gespreksresultaat,
  ContactmomentObject,
  Contactmoment,
  ContactverzoekDetail,
  ZaakContactmoment,
  ObjectContactmoment,
  KlantContactmoment,
} from "./types";
import type { ContactmomentViewModel } from "../shared/types";
import { toRelativeProxyUrl } from "@/helpers/url";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const objectcontactmomentenUrl = `${contactmomentenBaseUrl}"/objectcontactmomenten`;
const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export const saveContactmoment = (
  data: Contactmoment
): Promise<{ id: string; url: string; gespreksId: string }> =>
  fetchLoggedIn("/api/contactmomenten/contactmomenten/api/v1/contactmomenten", {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());

export const useGespreksResultaten = () => {
  const fetchBerichten = (url: string) =>
    fetchLoggedIn(url)
      .then((r) => {
        if (!r.ok) {
          throw new Error(
            "Er is een fout opgetreden bij het laden van de gespreksresultaten"
          );
        }
        return r.json();
      })
      .then((results) => {
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(results));
        return results as Array<Gespreksresultaat>;
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

export const koppelZaakContactmoment = (data: ZaakContactmoment) =>
  fetchLoggedIn(zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  }).then(throwIfNotOk);

export function koppelKlant({
  klantId,
  contactmomentId,
}: {
  klantId: string;
  contactmomentId: string;
}) {
  return fetchLoggedIn(contactmomentenBaseUrl + "klantcontactmomenten", {
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

export function useContactverzoekenByKlantId(
  id: Ref<string>,
  page: Ref<number>
) {
  function getUrl() {
    const url = new URL(window.gatewayBaseUri + "/api/klantcontactmomenten");
    url.searchParams.set(
      "_order[embedded.contactmoment.registratiedatum]",
      "desc"
    );
    url.searchParams.append("extend[]", "medewerker");
    url.searchParams.append("extend[]", "embedded._self.owner");
    url.searchParams.append("extend[]", "embedded.contactmoment.todo");
    url.searchParams.set("_limit", "10");
    url.searchParams.set("_page", page.value.toString());
    url.searchParams.set("embedded.klant._self.id", id.value);
    url.searchParams.set("embedded.contactmoment.todo", "IS NOT NULL");
    return url.toString();
  }

  return ServiceResult.fromFetcher(getUrl, fetchContactverzoeken, {
    getUniqueId() {
      return getUrl() + "contactverzoek";
    },
  });
}

export function useContactmomentenByKlantId(
  id: Ref<string>,
  page: Ref<number>
) {
  //get url for klantcontactmomenten

  function getUrl() {
    const searchParams = new URLSearchParams();
    searchParams.set(
      "klant",
      "https://open-klant.dev.kiss-demo.nl/klanten/api/v1/klanten/" + id.value
    );
    return `${klantcontactmomentenUrl}?${searchParams.toString()}`;
  }

  const mapObjectContactmoment = (a: any) => {
    return a as ObjectContactmoment;
  };

  const objectcontactmomenten = async (
    x: any,
    objectcontactmomenten: Array<any>
  ) => {
    const zaken = [];

    for (const item of objectcontactmomenten) {
      const relUrl = toRelativeProxyUrl(item, contactmomentenProxyRoot);
      if (relUrl) {
        const objectcontactmoment = await fetchLoggedIn(relUrl)
          .then(throwIfNotOk)
          .then((r) => r.json())
          .then((x) => mapObjectContactmoment(x));

        if (objectcontactmoment.objectType === "zaak") {
          zaken.push(objectcontactmoment.object);
        }
      }
    }

    return { ...x, zaken };
  };

  const mapKlantContactmoment = async (
    r: any
  ): Promise<KlantContactmoment | undefined> => {
    return {
      ...r,
    };
  };

  const fetchContactMomenten = (url: string) => {
    let relUrl;

    try {
      relUrl = new URL(url).pathname;
    } catch {
      return;
    }

    return fetchLoggedIn("/api/contactmomenten" + relUrl)
      .then(throwIfNotOk)
      .then((r) => r.json())
      .then((r) => objectcontactmomenten(r, r.objectcontactmomenten))
      .then((r) => r as ContactmomentViewModel);
  };

  function fetchKlantContactmomenten(url: any) {
    const klantContactmomentPage = fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((r) => r.json())
      .then((x) => parsePagination(x, mapKlantContactmoment))
      .then((paginated) => {
        const page = paginated.page.filter(Boolean) as KlantContactmoment[];

        const contactmomentenFetches: Array<Promise<ContactmomentViewModel>> =
          [];
        for (const item of page) {
          const contactmomentenFetch = fetchContactMomenten(item.contactmoment);
          if (contactmomentenFetch) {
            contactmomentenFetches.push(contactmomentenFetch);
          }
        }

        return Promise.all(contactmomentenFetches);
      });

    return klantContactmomentPage;
    //dit haalt eerst de klantcontactmomenten open. is een gepaginerder set (we gan uit van pagina 1, per item moeten we het contactmoment ophalen.
  }

  return ServiceResult.fromFetcher(getUrl, fetchKlantContactmomenten, {});
}

function fetchContactverzoeken(url: string): Promise<Paginated<any>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json())
    .then((x) => parsePagination(x, mapContactverzoekDetail))
    .then((paginated) => {
      const page = paginated.page.filter((p) => p !== undefined);

      return { ...paginated, page };
    });
}

const mapContactverzoekDetail = (
  rawContactverzoek: any
): ContactverzoekDetail | undefined => {
  const contactmoment = rawContactverzoek.embedded.contactmoment;
  const todo = rawContactverzoek.embedded.contactmoment.embedded.todo;

  return {
    id: contactmoment.id,
    datum: formatDateOnly(new Date(contactmoment.registratiedatum)),
    status: todo.completed ? "Gesloten" : "Open",
    behandelaar: todo.attendees?.[0] ?? "-",
    afgerond: todo.completed ? formatDateOnly(new Date(todo.completed)) : "-",
    starttijd: formatTimeOnly(new Date(contactmoment.registratiedatum)),
    aanmaker: contactmoment["_self"].owner,
    notitie: todo.description,
    primaireVraagWeergave: contactmoment.primaireVraagWeergave,
    afwijkendOnderwerp: contactmoment.afwijkendOnderwerp,
  };
};
