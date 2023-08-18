import { describe, expect, test } from "vitest";

import { parseDutchDate, parseBsn } from "@/helpers/validation";

describe("parseDutchDate", () => {
  test("should allow dd/MM/yyyy", async () => {
    const parsed = parseDutchDate("20/10/2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });
  test("should allow d/M/yyyy", async () => {
    const parsed = parseDutchDate("2/1/2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(2);
    expect(geboortedatum.getMonth()).toBe(0);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow dd-MM-yyyy", async () => {
    const parsed = parseDutchDate("20-10-2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow d-M-yyyy", async () => {
    const parsed = parseDutchDate("2-1-2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(2);
    expect(geboortedatum.getMonth()).toBe(0);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow ddMMyyyy", async () => {
    const parsed = parseDutchDate("20102000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should return an error on invalid date", async () => {
    const parsed = parseDutchDate("50-40-2000");
    expect(parsed).toBeInstanceOf(Error);
    const error = parsed as Error;
    expect(error.message).toBe(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
    );
  });

  test("should return an error on a non-date", async () => {
    const parsed = parseDutchDate("aaa bbb");
    expect(parsed).toBeInstanceOf(Error);
    const error = parsed as Error;
    expect(error.message).toBe(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
    );
  });
});

describe("parseBsn", () => {
  test("should allow a valid bsn", async () => {
    const parsed = parseBsn("645791258");
    expect(parsed).toBe("645791258");
  });

  test("should return an error on an invalid bsn", async () => {
    const parsed = parseBsn("123456789");
    const parsedError = parsed as Error;
    expect(parsedError.message).toBe("Dit is geen valide BSN.");
  });

  test("should return an error on an invalid bsn", async () => {
    const parsed = parseBsn("123");
    const parsedError = parsed as Error;
    expect(parsedError.message).toBe("Voer een BSN in van negen cijfers.");
  });
});
