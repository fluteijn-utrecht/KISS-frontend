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

export function parseDutchDate(input: string): Date | Error {
  const dateRegex =
    /^(([0-9]{2})([0-9]{2})([0-9]{4}))|(([0-9]{1,2})[/|-]([0-9]{1,2})[/|-]([0-9]{4}))$/;
  const matches = input?.trim().match(dateRegex);

  if (matches?.length !== 9) {
    return new Error(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
    );
  }

  const year = +(matches[4] || matches[8]);
  const month = +(matches[3] || matches[7]) - 1;
  const day = +(matches[2] || matches[6]);

  return new Date(year, month, day);
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
  const [left, right] = input.split(" ");
  if (!left || !right)
    return new Error(
      "Vul de geslachtsnaam en de geboortedatum in, gescheiden door een spatie"
    );
  let [geslachtsnaam, geboortedatum] = [left, parseDutchDate(right)];
  if (geboortedatum instanceof Error) {
    [geslachtsnaam, geboortedatum] = [right, parseDutchDate(left)];
  }
  if (geboortedatum instanceof Error) return geboortedatum;
  return {
    geboortedatum,
    geslachtsnaam,
  };
}
