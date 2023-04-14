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
} from "./types";

export const saveContactmoment = (
  data: Contactmoment
): Promise<{ id: string; url: string; gespreksId: string }> =>
  fetchLoggedIn(window.gatewayBaseUri + "/api/contactmomenten", {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());

const gespreksResultatenBaseUri =
  window.gatewayBaseUri + "/api/ref/resultaattypeomschrijvingen";

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
      .then((json) => {
        const results = json?.results;
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(json));
        return results as Array<Gespreksresultaat>;
      });

  return ServiceResult.fromFetcher(gespreksResultatenBaseUri, fetchBerichten);
};

const objectcontactmomentenUrl =
  window.gatewayBaseUri + "/api/objectcontactmomenten";

export const koppelObject = (data: ContactmomentObject) =>
  fetchLoggedIn(objectcontactmomentenUrl, {
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
  return fetchLoggedIn(window.gatewayBaseUri + "/api/klantcontactmomenten", {
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
