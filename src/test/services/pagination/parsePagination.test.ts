import { flushPromises } from "@vue/test-utils";
import {
  ServiceResult,
  fetchLoggedIn,
  mapServiceData,
  parseJson,
  throwIfNotOk,
  type ServiceData,
  parsePagination,
} from "@/services";

import {
  assertType,
  describe,
  expect,
  expectTypeOf,
  test,
  type Test,
} from "vitest";

describe("pagination-test", () => {
  test("test if a synchronous mapped object ...", async () => {
    // type TestType = {
    //   data: string;
    // };

    const pageData = [{ data: "a" }, { data: "b" }];
    const nextUrl = "nextUrl";
    const previousUrl = "previousUrl";

    const result = await parsePagination(
      {
        count: 2,
        next: nextUrl,
        previous: previousUrl,
        results: pageData,
      },
      (obj: unknown): unknown => obj
    );

    //  await flushPromises();

    expect(result.count).toBe(2);
    expect(result.next).toBe(nextUrl);
    expect(result.previous).toBe(previousUrl);
    expect(result.page).toStrictEqual(pageData);
  });
});
