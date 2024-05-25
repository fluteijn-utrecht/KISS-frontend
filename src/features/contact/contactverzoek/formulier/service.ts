import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";
import {
  typeActorOptions,
  type ContactmomentContactVerzoek,
} from "@/stores/contactmoment";
import { formatIsoDate } from "@/helpers/date";
import { fullName } from "@/helpers/string";
import type {
  ContactVerzoekVragenSet,
  Vraag,
  InputVraag,
  TextareaVraag,
  DropdownVraag,
  CheckboxVraag,
} from "./types";
import type {
  ContactverzoekData,
  DigitaalAdres,
  NewContactverzoek,
} from "../types";
import { TypeOrganisatorischeEenheid } from "../types";

const contactMomentVragenSets = "/api/contactverzoekvragensets";

type ServerContactVerzoekVragenSet = {
  id: number;
  titel: string;
  jsonVragen: string;
  afdelingId: string;
};

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
      typeVersion: 3, //todo configureerbaar
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

  if (data.typeActor == typeActorOptions.groep) {
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
  if (data.typeActor == typeActorOptions.afdeling) {
    console.log("data.medewerker", data.medewerker);
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
  if (data.typeActor == typeActorOptions.medewerker) {
    actor = {
      naam: data.medewerker?.achternaam || "",
      soortActor: "medewerker",
      identificatie: data.medewerker?.identificatie || "",
      typeOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.naam.split(": ")[0] ==
        "afdeling"
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

export function fetchVragenSets(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((response) => response.json())
    .then((data) => {
      return mapToClientContactVerzoekVragenSets(data);
    });
}

export function useVragenSets() {
  return ServiceResult.fromFetcher(
    () => contactMomentVragenSets,
    fetchVragenSets,
  );
}

function mapToClientContactVerzoekVragenSets(
  serverDataArray: ServerContactVerzoekVragenSet[],
): ContactVerzoekVragenSet[] {
  return serverDataArray.map((serverData) => {
    const parsedQuestions = mapSchemaToVragen(
      safeJSONParse(serverData.jsonVragen, {}),
    );
    return {
      id: serverData.id,
      titel: serverData.titel,
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
    "description" in question &&
    "input" in question &&
    question.description.trim() !== "" &&
    question.questiontype === "input"
  );
}

export function isTextareaVraag(question: Vraag): question is TextareaVraag {
  return (
    question.description.trim() !== "" &&
    "description" in question &&
    question.questiontype === "textarea" &&
    "textarea" in question
  );
}

export function isDropdownVraag(question: Vraag): question is DropdownVraag {
  return (
    question.questiontype === "dropdown" &&
    "description" in question &&
    question.description.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}

export function isCheckboxVraag(question: Vraag): question is CheckboxVraag {
  return (
    question.questiontype === "checkbox" &&
    "description" in question &&
    question.description.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapSchemaToVragen(schema: any): Vraag[] {
  if (!schema || !schema.properties) {
    return [];
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return Object.values(schema.properties).map((property: any) => {
    const questionType = property.questiontype;

    const baseVraag: Vraag = {
      description: property.description,
      questiontype: questionType,
    };

    switch (questionType) {
      case "dropdown":
        return {
          ...baseVraag,
          options: property.items?.options || [],
          selectedDropdown: "",
        } as DropdownVraag;

      case "checkbox":
        return {
          ...baseVraag,
          options: property.items?.options || [],
          selectedCheckbox: [],
        } as CheckboxVraag;

      case "input":
        return { ...baseVraag, input: "" } as InputVraag;

      case "textarea":
        return { ...baseVraag, textarea: "" } as TextareaVraag;

      default:
        return baseVraag;
    }
  });
}
