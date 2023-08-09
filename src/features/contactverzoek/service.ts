import {
  fetchLoggedIn,
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

export interface Contactverzoek {
  bronorganisatie: string; //verplicht in de api
  todo: {
    name: "contactverzoek";
    description: string;
    attendees: string[];
  };
  vraag?: string;
  specifiekevraag?: string;
}

export const useContactverzoekObjectTypeUrl = ServiceResult.fromFetcher(
  "/api/internetaak/objecttypeurl",
  (url) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((r) => r.text())
);

export function saveContactverzoek({
  data,
  contactmomentUrl,
  typeUrl,
  klantUrl,
  persoonsnaam,
  organisatie,
}: {
  data: ContactmomentContactVerzoek;
  contactmomentUrl: string;
  typeUrl: string;
  klantUrl: string;
  persoonsnaam?: {
    voornaam: string;
    achternaam: string;
    voorvoegselAchternaam?: string;
  };
  organisatie?: string | undefined;
}) {
  const url = "/api/internetaak/api/v2/objects";
  const registratiedatum = new Date().toISOString();
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

  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      type: typeUrl,
      record: {
        typeVersion: 0,
        startAt: registratiedatum,
        data: {
          contactmoment: contactmomentUrl,
          registratiedatum,
          toelichting: data.interneToelichting,
          actor: {
            identificatie: data.medewerker,
            soortActor: "medewerker",
          },
          betrokkene: {
            rol: "klant",
            klant: klantUrl,
            persoonsnaam,
            organisatie,
            digitaleAdressen,
          },
        },
      },
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json() as Promise<{ id: string; url: string }>);
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
