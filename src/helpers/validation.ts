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
}

export interface GeslachtsnaamGeboortedatum {
  geslachtsnaam: string;
  geboortedatum: Date;
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

  if (!year || !month || !day)
    return new Error(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
    );

  return {
    date: new Date(+year, +month - 1, +day),
    matchedString: matches?.[0] ?? "",
  };
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

export function parseGeslachtsnaamGeboortedatum(
  input: string
): GeslachtsnaamGeboortedatum | Error {
  const result = matchDutchDate(input);

  if (result instanceof Error) return result;

  const geslachtsnaam = input.replace(result.matchedString, "").trim();

  if (!geslachtsnaam) return new Error("Vul een achternaam in");
  return {
    geboortedatum: result.date,
    geslachtsnaam,
  };
}
