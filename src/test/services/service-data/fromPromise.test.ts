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
  test("ServiceResult.fromPromise: test if the correct result is returned for a resolved Promise.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromPromise<TestType>(
      Promise.resolve({ data: "value" } as TestType)
    );

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    await flushPromises();

    expect(fromPromiseResult.state).toMatch("success");
    expect(fromPromiseResult.success).toBeTruthy();
  });

  test("ServiceResult.fromPromise: test if the correct result is returned for a pending Promise.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromPromise<TestType>(
      Promise.resolve({ data: "value" } as TestType)
    );

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    expect(fromPromiseResult.state).toMatch("loading");
    expect(fromPromiseResult.success).toBeFalsy();
  });

  test("ServiceResult.fromPromise: test if the correct result  is returned for a rejected Promise.", async () => {
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
