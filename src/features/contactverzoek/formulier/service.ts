import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type { ContactmomentContactVerzoek, ContactverzoekGroep } from "@/stores/contactmoment";
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
import type { ContactverzoekData, NewContactverzoek } from "../types";
import { useAfdelingen } from "@/composables/afdelingen";
import { useGroepen } from "@/composables/groepen";
import { computed } from 'vue'

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

  return {
    status: "te verwerken",
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
  };
}

// export function useAfdelingenGroepen(search: () => string | undefined): ComputedRef<(Afdeling | Groep)[]> {
//     const afdelingen = useAfdelingen(search);
//     const groepen = useGroepen(search);

//     // Combining data with prefixes and computed to be reactive
//     const combinedData = computed(() => {
//         const afdelingenWithPrefix = afdelingen.value.map(afdeling => ({
//             ...afdeling,
//             naam: `afdeling: ${afdeling.naam}`
//         }));

//         const groepenWithPrefix = groepen.value.map(groep => ({
//             ...groep,
//             naam: `groep: ${groep.naam}`
//         }));

//         return [...afdelingenWithPrefix, ...groepenWithPrefix];
//     });

//     return combinedData;
// }

// export function useAfdelingenGroepen(search: () => string | undefined) {
//   const afdelingen = useAfdelingen(search);
//   const groepen = useGroepen(search);

//   const combinedData = computed(() => {
//       // Combine data from both sources
//       return [...afdelingen.value, ...groepen.value];
//   });

//   return combinedData;
// }


// export function useAfdelingenGroepen(search: () => string | undefined) {
//   const afdelingen = useAfdelingen(search);
//   const groepen = useGroepen(search);

//   // Return the result directly or through a method, not a computed
//   return () => {
//     if (afdelingen.success && groepen.success) {
//       const afdelingenList = afdelingen.data.page.map(item => "Afdeling: " + item.naam);
//       const groepenList = groepen.data.page.map(item => "Groep: " + item.naam);
//       return [...afdelingenList, ...groepenList];
//     } else {
//       return [];
//     }
//   };
// }

export function useAfdelingenGroepen(afdelingenNames: string[], groepenNames: string[]) {
  const results: string[] = [];

  // Process each afdeling
  afdelingenNames.forEach(afdeling => {
    const afdelingen = useAfdelingen(() => undefined);
    if (afdelingen.success) {
      const afdelingenList = afdelingen.data.page.map(item => "Afdeling: " + item.naam);
      results.push(...afdelingenList);
    }
  });

  // Process each groep
  groepenNames.forEach(groep => {
    const groepen = useGroepen(() => undefined);
    if (groepen.success) {
      const groepenList = groepen.data.page.map(item => "Groep: " + item.naam);
      results.push(...groepenList);
    }
  });

  return results;
}

// export function useSearchAfdelingenGroepen(search: () => string | undefined) {
//   const afdelingen = useAfdelingen(search);
//   const groepen = useGroepen(search);

//   const combinedData = computed(() => {
//     if (afdelingen.success && groepen.success) {
//       const afdelingenList = afdelingen.data.page.map((item) => ({
//         naam: "Afdeling: " + item.naam,
//         identificatie: item.id 
//       }));
//       const groepenList = groepen.data.page.map((item) => ({
//         naam: "Groep: " + item.naam,
//         identificatie: item.id
//       }));
//       return [...afdelingenList, ...groepenList];
//     }
//     return [];
//   });

//   return { 
//     data: combinedData,
//     success: computed(() => afdelingen.success && groepen.success)
//   };
// }
// export function useGroepen(search?: () => string | undefined) {
//   const getUrl = () => {
//     const searchParams = new URLSearchParams();
//     searchParams.set("ordering", "record__data__naam");

//     // Initialize an empty array for dynamic query parameters
//     const data_attrs = [];

//     // Retrieve the search string if the search function is defined
//     const searchStr = search?.();
//     if (searchStr) {
//       data_attrs.push(`naam__icontains__${searchStr}`);
//     }

//     // Only add the data_attrs parameter if there are any conditions to add
//     if (data_attrs.length > 0) {
//       searchParams.set("data_attrs", data_attrs.join(","));
//     }

//     return "/api/groepen/api/v2/objects?" + searchParams.toString();
//   };

//   const mapOrganisatie = (x: any) => x.record.data as ContactverzoekGroep;

//   const fetcher = (url: string): Promise<PaginatedResult<ContactverzoekGroep>> =>
//     fetchLoggedIn(url)
//       .then(throwIfNotOk)
//       .then(parseJson)
//       .then((json) => parsePagination(json, mapOrganisatie));

//   return ServiceResult.fromFetcher(getUrl, fetcher);
// }

// export function useGroepenByAfdelingId(
//   getAfdelingId: () => string | undefined,
//   search?: () => string | undefined,
// ) {
//   const getUrl = () => {
//     const afdelingId = getAfdelingId();
//     if (!afdelingId) return "";
//     const searchParams = new URLSearchParams();
//     searchParams.set("ordering", "record__data__naam");
//     const data_attrs = [`afdelingId__exact__${afdelingId}`];

//     const searchStr = search?.();
//     if (searchStr) {
//       data_attrs.push(`naam__icontains__${searchStr}`);
//     }

//     searchParams.set("data_attrs", data_attrs.join(","));

//     return "/api/groepen/api/v2/objects?" + searchParams;
//   };

//   const mapOrganisatie = (x: any) => x.record.data as ContactverzoekGroep;

//   const fetcher = (url: string): Promise<PaginatedResult<ContactverzoekGroep>> =>
//     fetchLoggedIn(url)
//       .then(throwIfNotOk)
//       .then(parseJson)
//       .then((json) => parsePagination(json, mapOrganisatie));

//   return ServiceResult.fromFetcher(getUrl, fetcher);
// }

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

function mapSchemaToVragen(schema: any): Vraag[] {
  if (!schema || !schema.properties) {
    return [];
  }

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
