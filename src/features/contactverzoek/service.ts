import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { formatIsoDate } from "@/helpers/date";
import type { Ref } from "vue";
import { fullName } from "@/helpers/string";
import type {
  ContactVerzoekVragenSet,
  Vraag,
  InputVraag,
  TextareaVraag,
  DropdownVraag,
  CheckboxVraag,
} from "./types";

const contactMomentVragenSets = "/api/contactverzoekvragensets";

type ServerContactVerzoekVragenSet = {
  id: string;
  naam: string;
  jsonVragen: string;
  afdelingId: string;
};

type NewContactverzoek = {
  record: {
    typeVersion: number;
    startAt: string;
    data: {
      status: string;
      contactmoment: string;
      registratiedatum: string;
      datumVerwerkt?: string;
      toelichting?: string;
      actor: {
        identificatie: string;
        soortActor: string;
        naam: string;
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

  function formatVraagAntwoordForToelichting(vraagAntwoord: Vraag[]): string {
    return vraagAntwoord
      .map((va) => {
        if (isInputVraag(va)) {
          return `${va.label}: ${va.input}`;
        } else if (isTextareaVraag(va)) {
          return `${va.label}: ${va.textarea}`;
        } else if (isDropdownVraag(va)) {
          return `${va.label}: ${va.selectedDropdown}`;
        } else if (isCheckboxVraag(va)) {
          const selectedOptions = va.options
            .filter((_, index) => va.selectedCheckbox[index])
            .join(", ");
          return `${va.label}: ${selectedOptions}`;
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

  const organisatorischeEenheid = data.groep
    ? {
        identificatie: data.groep.identificatie,
        naam: data.groep.naam,
        soortActor: "organisatorische eenheid",
      }
    : {
        identificatie: data.afdeling?.identificatie || "",
        naam: data.afdeling?.naam || "",
        soortActor: "organisatorische eenheid",
      };

  const actor = data.isMedewerker
    ? {
        identificatie: data.medewerker?.identificatie || "",
        naam: fullName(data.medewerker),
        soortActor: "medewerker",
      }
    : organisatorischeEenheid;

  const body: NewContactverzoek = {
    record: {
      typeVersion: 1,
      startAt,
      data: {
        status: "te verwerken",
        contactmoment: contactmomentUrl,
        registratiedatum,
        toelichting:
          data.interneToelichting +
          (vragenToelichting ? "\n\n" + vragenToelichting : ""),
        actor,
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
    .then((r) => r.json() as Promise<{ url: string }>);
}

export function useContactverzoekenByKlantId(
  id: Ref<string>,
  page: Ref<number>,
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

interface Afdeling {
  id: string;
  identificatie: string;
  naam: string;
}

interface Groep {
  identificatie: string;
  naam: string;
  afdelingId: string;
}

export function useAfdelingen(search: () => string | undefined) {
  const getUrl = () => {
    const searchParams = new URLSearchParams();
    searchParams.set("ordering", "record__data__naam");
    const searchStr = search();
    if (searchStr) {
      searchParams.set("data_attrs", `naam__icontains__${searchStr}`);
    }
    return "/api/afdelingen/api/v2/objects?" + searchParams;
  };

  const mapOrganisatie = (x: any) =>
    ({
      ...x.record.data,
      id: x.uuid,
    }) as Afdeling;

  const fetcher = (url: string): Promise<PaginatedResult<Afdeling>> =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((json) => parsePagination(json, mapOrganisatie));

  return ServiceResult.fromFetcher(getUrl, fetcher);
}

export function useGroepen(
  getAfdelingId: () => string | undefined,
  search?: () => string | undefined,
) {
  const getUrl = () => {
    const afdelingId = getAfdelingId();
    if (!afdelingId) return "";
    const searchParams = new URLSearchParams();
    searchParams.set("ordering", "record__data__naam");
    searchParams.set("data_attrs", `afdelingId__exact__${afdelingId}`);

    const searchStr = search?.();
    if (searchStr) {
      searchParams.set("data_attrs", `naam__icontains__${searchStr}`);
    }

    return "/api/groepen/api/v2/objects?" + searchParams;
  };

  const mapOrganisatie = (x: any) => x.record.data as Groep;

  const fetcher = (url: string): Promise<PaginatedResult<Groep>> =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((json) => parsePagination(json, mapOrganisatie));

  return ServiceResult.fromFetcher(getUrl, fetcher);
}

export function useVragenSets() {
  return ServiceResult.fromFetcher(
    () => contactMomentVragenSets,
    fetchVragenSets,
  );
}

export function fetchVragenSets(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((response) => response.json())
    .then((data) => {
      return mapToClientContactVerzoekVragenSets(data);
    });
}

function mapToClientContactVerzoekVragenSets(
  serverDataArray: ServerContactVerzoekVragenSet[],
): ContactVerzoekVragenSet[] {
  return serverDataArray.map((serverData) => {
    const parsedQuestions = safeJSONParse<Vraag[]>(serverData.jsonVragen, []);

    // Initialize selectedCheckbox array for CheckboxVraag type questions
    parsedQuestions.forEach((question) => {
      if (isCheckboxVraag(question)) {
        question.selectedCheckbox = Array(question.options.length).fill(false);
      }
    });

    return {
      id: serverData.id,
      naam: serverData.naam,
      vraagAntwoord: parsedQuestions,
      afdelingId: serverData.afdelingId,
    };
  });
}

function safeJSONParse<T>(jsonString: string, defaultValue: T): T {
  try {
    return JSON.parse(jsonString);
  } catch (e) {
    return defaultValue;
  }
}

export function isInputVraag(question: Vraag): question is InputVraag {
  return (
    "label" in question &&
    question.label.trim() !== "" &&
    question.type === "input"
  );
}

export function isTextareaVraag(question: Vraag): question is TextareaVraag {
  return (
    "label" in question &&
    question.label.trim() !== "" &&
    question.type === "textarea"
  );
}

export function isDropdownVraag(question: Vraag): question is DropdownVraag {
  return (
    question.type === "dropdown" &&
    "label" in question &&
    question.label.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}

export function isCheckboxVraag(question: Vraag): question is CheckboxVraag {
  return (
    question.type === "checkbox" &&
    "label" in question &&
    question.label.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}
