import type { Directive } from "vue";
import { watch } from "vue";

function isValidPhoneNumber(val: string) {
  if (!val) return true; // empty is allowed, handeld by required attribute
  const numberCount = (val.match(/[0-9]/g) ?? []).length;
  const hasOnlyAllowedChars = /^(\+|-| |[0-9])*$/.test(val);
  return numberCount >= 10 && hasOnlyAllowedChars;
}

export function customPhoneValidator(value: string): string[] {
  return isValidPhoneNumber(value)
    ? []
    : [`'${value}' lijkt geen valide telefoonnummer.`];
}
interface Postcode {
  numbers: string;
  digits: string;
}

export interface PostcodeHuisnummer {
  postcode: Postcode;
  huisnummer: string;
  toevoeging?: string;
}

export interface GeslachtsnaamGeboortedatum {
  geslachtsnaam: string;
  geboortedatum: Date;
}

export function parsePostcode(input: string): Postcode | Error {
  const matches = input.match(/^ *([1-9]\d{3}) *([A-Za-z]{2}) *$/);
  if (matches?.length !== 3) {
    return new Error("Voer een valide postcode in, bijvoorbeeld 1234 AZ.");
  }
  return {
    numbers: matches[1],
    digits: matches[2],
  };
}

export function parsePostcodeHuisnummer(
  input: string
): PostcodeHuisnummer | Error {
  const matches = input
    .match(/^ *([1-9]\d{3}) *([A-Za-z]{2}) *(\d*) *$/)
    ?.filter(Boolean);

  if (matches?.length !== 4) {
    return new Error(
      "Voer een valide postcode en huisnummer in, bijvoorbeeld 1234 AZ 12."
    );
  }

  return {
    postcode: {
      numbers: matches[1],
      digits: matches[2],
    },
    huisnummer: matches[3],
  };
}

const dateRegex =
  /((?<day1>[0-9]{2})(?<month1>[0-9]{2})(?<year1>[0-9]{4}))|((?<day2>[0-9]{1,2})[/|-](?<month2>[0-9]{1,2})[/|-](?<year2>[0-9]{4}))/;

const matchDutchDate = (input: string) => {
  const trimmed = input?.trim();
  const matches = trimmed.match(dateRegex);

  const { year1, month1, day1, year2, month2, day2 } = matches?.groups ?? {};
  const year = year1 || year2;
  const month = month1 || month2;
  const day = day1 || day2;

  if (year && month && day) {
    const correctedMonth = +month - 1;
    const intYear = +year;
    const intDay = +day;
    const date = new Date(intYear, correctedMonth, intDay);
    if (
      date.getFullYear() === intYear &&
      date.getMonth() === correctedMonth &&
      date.getDate() === intDay
    )
      return {
        date,
        matchedString: matches?.[0] ?? "",
      };
  }

  return new Error(
    "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
  );
};

export function parseDutchDate(input: string): Date | Error {
  const result = matchDutchDate(input);
  return result instanceof Error ? result : result.date;
}

const multipliers = [9, 8, 7, 6, 5, 4, 3, 2, -1] as const;

function elfProef(numbers: number[]): boolean {
  if (numbers.length !== 9) return false;
  const multipliedSum = numbers.reduce(
    (previousValue, currentValue, currentIndex) =>
      previousValue + currentValue * multipliers[currentIndex],
    0
  );
  return multipliedSum % 11 === 0;
}

export function parseBsn(input: string): string | Error {
  if (!input) return input;
  const matches = input.match(/^ *(\d{9}) *$/);
  if (!matches || matches.length < 2)
    return new Error("Voer een BSN in van negen cijfers.");
  const match = matches[1];
  const numbers = match.split("").map((char) => +char);
  return elfProef(numbers) ? match : new Error("Dit is geen valide BSN.");
}

export function parseKvkNummer(input: string): string | Error {
  const matches = input.match(/^ *(\d{8}) *$/);
  return !matches || matches.length < 2
    ? new Error("Vul de 8 cijfers van het KvK-nummer in, bijvoorbeeld 12345678")
    : matches[1];
}

export type Validator<T> = (v: string) => T | Error;
export type ValidatorSetup<T = any> = {
  validator: Validator<T>;
  current: string;
  validated: T | undefined;
};

export function validateWith<T>(validator: Validator<T>): ValidatorSetup<T> {
  return {
    validator,
    current: "",
    validated: undefined,
  };
}

export const vValidate: Directive<HTMLInputElement, ValidatorSetup> = {
  mounted(el, { value }) {
    const unwatch = watch(
      () => value.current,
      (i) => {
        el.value = i;
        const parsed = value.validator(i);
        if (parsed instanceof Error) {
          value.validated = undefined;
          el.setCustomValidity(parsed.message);
        } else {
          el.setCustomValidity("");
          value.validated = parsed;
        }
      },
      { immediate: true }
    );
    function onInput() {
      value.current = el.value;
    }
    el.addEventListener("input", onInput);
    (el as any).removeValidatorSetup = () => {
      el.removeEventListener("input", onInput);
      unwatch();
    };
  },
  unmounted(el) {
    (el as any).removeValidatorSetup?.();
  },
};
