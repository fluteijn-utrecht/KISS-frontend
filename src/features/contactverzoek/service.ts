import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  // parseJson,
  // parsePagination,
  ServiceResult,
  throwIfNotOk,
} from "@/services";
import type {
  ContactmomentContactVerzoek,
  NieuweKlant,
} from "@/stores/contactmoment";
// creating a klant will be done differently in the future. for now, jus reuse the type from the klant feature
import { KlantType } from "../klant/types";
import { formatIsoDate } from "@/helpers/date";
import type { Ref } from "vue";

type NewContactverzoek = {
  record: {
    typeVersion: number;
    startAt: string;
    data: {
      status: string;
      contactmoment: string;
      registratiedatum: string;
      toelichting?: string;
      actor: {
        identificatie: string;
        soortActor: "medewerker";
      };
      betrokkene: {
        rol: "klant";
        klant?: string;
        persoonsnaam?: {
          voornaam?: string;
          voorvoegselAchternaam?: string;
          achternaam?: string;
        };
        organisatie?: string;
        digitaleAdressen: {
          adres: string;
          soortDigitaalAdres?: string;
          omschrijving?: string;
        }[];
      };
    };
  };
};

export type Contactverzoek = NewContactverzoek & {
  url: string;
};

export function saveContactverzoek({
  data,
  contactmomentUrl,
  klantUrl,
}: {
  data: ContactmomentContactVerzoek;
  contactmomentUrl: string;
  klantUrl?: string;
}) {
  const url = "/api/internetaak/api/v2/objects";
  const now = new Date();
  const registratiedatum = now.toISOString();
  const startAt = formatIsoDate(now);
  const digitaleAdressen = [] as any[];
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

  const body: NewContactverzoek = {
    record: {
      typeVersion: 0,
      startAt,
      data: {
        status: "te verwerken",
        contactmoment: contactmomentUrl,
        registratiedatum,
        toelichting: data.interneToelichting,
        actor: {
          identificatie: data.medewerker || "",
          soortActor: "medewerker",
        },
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
    .then((r) => r.json() as Promise<{ id: string; url: string }>);
}

export function useContactverzoekenByKlantId(
  id: Ref<string>,
  page: Ref<number>
) {
  function getUrl() {
    if (!id.value) return "";
    const url = new URL("/api/internetaak/api/v2/objects", location.href);
    url.searchParams.set("ordering", "-record__data__registratiedatum");
    url.searchParams.set("pageSize", "10");
    url.searchParams.set("page", page.value.toString());
    url.searchParams.set("data_attrs", `betrokkene__klant__exact__${id.value}`);
    return url.toString();
  }

  const fetchContactverzoeken = (url: string) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) => parsePagination(r, (v) => v as Contactverzoek));

  return ServiceResult.fromFetcher(getUrl, fetchContactverzoeken);
}

export function createKlant(klant: NieuweKlant) {
  return Promise.reject("not implemented");
  const url = "/api/klanten";
  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      ...klant,
      bronorganisatie,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectType: KlantType.Persoon,
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());
}

// interface Afdeling {
//   id: string;
//   name: string;
// }

export function useAfdelingen() {
  const url = "not implemented";

  // const mapOrganisatie = (x: unknown): Afdeling => x as any;

  // const fetcher = (url: string, page = 1, limit = 100): Promise<Afdeling[]> =>
  //   fetchLoggedIn(`${url}?_limit=${limit}&_page=${page}`)
  //     .then(throwIfNotOk)
  //     .then(parseJson)
  //     .then((json) => parsePagination(json, mapOrganisatie))
  //     .then(async (current) => {
  //       //paginering model is gewijzigt daarom, vooruitlopend op de refactoring van dit deel, deze temp fix
  //       return current.page;
  //       // if (current.totalPages <= current.pageNumber) return current.page;
  //       // const nextPage = await fetcher(url, page + 1);
  //       // return [...current.page, ...nextPage];
  //     })
  //     .then((all) => all.sort((a, b) => a.name.localeCompare(b.name)));

  return ServiceResult.fromFetcher(url, () =>
    Promise.reject("not implemented")
  );
}
