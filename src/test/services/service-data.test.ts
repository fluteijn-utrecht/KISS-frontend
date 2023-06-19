import { flushPromises } from "@vue/test-utils";
import {
  ServiceResult,
  fetchLoggedIn,
  mapServiceData,
  parseJson,
  throwIfNotOk,
} from "@/services";

import { describe, expect, expectTypeOf, test } from "vitest";

describe("service-data-test", () => {
  test("ServiceResult.fromPromise: test if the correct result type is returned for a resolved Promise.", async () => {
    let result = false;

    type TestType = {
      data: string;
    };

    const testPromise = Promise.resolve({ data: "value" } as TestType);
    const y = await ServiceResult.fromPromise<TestType>(
      testPromise.then((r) => {
        result = true;
        console.log(123);
        return r;
      })
    );

    await flushPromises();

    expect(result).toEqual(true);
    expectTypeOf(y).toMatchTypeOf<TestType>();
  });

  test("ServiceResult.fromPromise: test if the correct result type is returned for a pending Promise.", async () => {
    let result = false;

    type TestType = {
      data: string;
    };

    const testPromise = new Promise<TestType>(() => {
      return {} as TestType;
    });
    const y = await ServiceResult.fromPromise<TestType>(
      testPromise.then((r) => {
        result = true;
        console.log(123);
        return r;
      })
    );

    await flushPromises();

    expect(result).toEqual(false); // if flase then not resolved yet.
    expectTypeOf(y).toMatchTypeOf<TestType>();
  });

  test("ServiceResult.fromPromise: test if the correct result type is returned for a rejected Promise.", async () => {
    let result = false;

    type TestType = {
      data: string;
    };

    const testPromise = Promise.reject({ data: "value" } as TestType);
    const y = await ServiceResult.fromPromise<TestType>(
      testPromise.then((r) => {
        result = true;
        console.log(123);
        return r;
      })
    );

    const u = ServiceResult.fromPromise<TestType>(
      testPromise.then((r) => {
        result = true;
        console.log(123);
        return r;
      })
    );

    await flushPromises();

    const suc = u.success;
    expect(suc).toEqual(false);
    //  expectTypeOf(y).toMatchTypeOf<TestType>();
  });
});
