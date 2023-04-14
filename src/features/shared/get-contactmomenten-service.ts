import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
} from "@/services";
import type { Ref } from "vue";
import type { ContactmomentZaak, ContactmomentViewModel } from "./types";

const mapZaak = (json: any): ContactmomentZaak => ({
  status: json?.embedded?.status?.statustoelichting,
  zaaktype: json?.embedded?.zaaktype?.onderwerp,
  zaaknummer: json?.identificatie,
});

const fixUrl = (object: string) =>
  object.startsWith("/") ? window.gatewayBaseUri + object : object;

const fetchObject = (
  {
    object,
  }: {
    object: string;
  },
  extendArr: string[] = []
) => {
  const url = new URL(fixUrl(object));
  for (const extend of extendArr ?? []) {
    url.searchParams.append("extend[]", extend);
  }
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((or) => or.json());
};

function mapContactverzoek(obj: any) {
  const todo = obj?.embedded?.todo;
  const completed = todo?.completed || "";

  return {
    medewerker: todo?.attendees[0] ?? "onbekend",
    completed: completed ? new Date(completed) : undefined,
  };
}

const mapContactmoment = async (
  r: any
): Promise<ContactmomentViewModel | undefined> => {
  const contactmoment = r.embedded.contactmoment as ContactmomentViewModel;

  contactmoment.startdatum = new Date(contactmoment.startdatum);
  contactmoment.registratiedatum = new Date(contactmoment.registratiedatum);

  const objectcontactmomenten: any[] =
    r.embedded?.contactmoment?.embedded?.objectcontactmomenten ?? [];

  const zakenPromises = objectcontactmomenten
    .filter(({ objectType }: any) => objectType === "zaak")
    .map((x) => fetchObject(x, ["status", "zaaktype"]).then(mapZaak));

  const contactverzoekPromises = objectcontactmomenten
    .filter(({ objectType }: any) => objectType === "contactmomentobject")
    .map((x) => fetchObject(x, ["todo"]).then(mapContactverzoek));

  return {
    ...contactmoment,
    zaken: await Promise.all(zakenPromises),
    contactverzoeken: await Promise.all(contactverzoekPromises),
  };
};

function fetchContactmomenten(
  url: string
): Promise<Paginated<ContactmomentViewModel>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json())
    .then((x) => parsePagination(x, mapContactmoment))
    .then((paginated) => {
      const page = paginated.page.filter(Boolean) as ContactmomentViewModel[];

      return {
        ...paginated,
        page,
      };
    });
}

export function useContactmomentenByZaakUrl(
  self: Ref<string>,
  page: Ref<number>
) {
  function getUrl() {
    const url = new URL(`${window.gatewayBaseUri}/api/objectcontactmomenten`);
    url.searchParams.set("_order[contactmoment.registratiedatum]", "desc");
    url.searchParams.append("extend[]", "embedded._self.owner");
    url.searchParams.append("extend[]", "all");
    url.searchParams.append("objectType", "zaak");
    url.searchParams.append("object", self.value);
    url.searchParams.append("_page", page.value.toString());
    return url.toString();
  }
  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten);
}

export function useContactmomentenByKlantId(
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
    url.searchParams.set("embedded.contactmoment.todo", "IS NULL");
    return url.toString();
  }
  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten, {
    getUniqueId() {
      return getUrl() + "contactmoment";
    },
  });
}
