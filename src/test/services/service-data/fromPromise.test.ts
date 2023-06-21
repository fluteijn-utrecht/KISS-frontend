import { flushPromises } from "@vue/test-utils";
import {
  ServiceResult,
  fetchLoggedIn,
  mapServiceData,
  parseJson,
  throwIfNotOk,
  type ServiceData,
} from "@/services";

import {
  assertType,
  describe,
  expect,
  expectTypeOf,
  test,
  type Test,
} from "vitest";

describe("service-data-test", () => {
  test("ServiceResult.fromPromise: test if the correct result type is returned for a resolved Promise.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = await ServiceResult.fromPromise<TestType>(
      Promise.resolve({ data: "value" } as TestType)
    );

    await flushPromises();

    expectTypeOf(fromPromiseResult).toMatchTypeOf<TestType>();
  });

  test("ServiceResult.fromPromise: test if the correct result type is returned for a pending Promise.", async () => {
    const result = false;

    type TestType = {
      data: string;
    };

    const testPromise = new Promise<TestType>(() => {
      return { data: "value" } as TestType;
    });

    const y = ServiceResult.fromPromise<TestType>(testPromise);

    await flushPromises();

    // expectTypeOf(y).toMatchTypeOf<TestType>();

    // Promise.resolve(testPromise);

    // expectTypeOf(y).toMatchTypeOf<TestType>();
  });

  test("ServiceResult.fromPromise: test if the correct result type is returned for a rejected Promise.", async () => {
    type TestType = {
      data: string;
    };

    let x;
    const y = ServiceResult.fromPromise<TestType>(
      (x = Promise.reject<TestType>())
    );

    await flushPromises();

    x.catch(async (err) => {
      expect(y.state).toMatch("error");
      expect(y.success).toBeFalsy();
    });
  });
});
