import { flushPromises, mount } from "@vue/test-utils";
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
  test("ServiceResult.fromSubmitter returns success for a resolved Promise.", async () => {
    type TestType = {
      data: string;
    };

    const fromSubmitterResult = ServiceResult.fromSubmitter<TestType, TestType>(
      () => Promise.resolve({ data: "out" } as TestType)
    );

    fromSubmitterResult.submit({ data: "in" });

    expectTypeOf(fromSubmitterResult).toMatchTypeOf<ServiceData<TestType>>();

    await flushPromises();

    expect(fromSubmitterResult.state).toMatch("success");
    expect(fromSubmitterResult.success).toBeTruthy();
    if (fromSubmitterResult.success) {
      expect((fromSubmitterResult.data as TestType).data).toMatch("out");
    }
  });

  test("ServiceResult.fromSubmitter returns a loading state for a pending Promise.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromSubmitter<TestType, TestType>(
      () => Promise.resolve({ data: "value" } as TestType)
    );

    fromPromiseResult.submit({ data: "in" });

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    expect(fromPromiseResult.state).toMatch("loading");
    expect(fromPromiseResult.success).toBeFalsy();
  });

  test("ServiceResult.fromSubmitter returns an eroor state for a rejected Promise.", async () => {
    type TestType = {
      data: string;
    };

    //de te testen code is gewrapped in een leeg vue component en wordt aangeroepen vanuit de mounted lifecycle hook
    //testen van rejected promises werkt dan beter
    mount(
      {},
      {
        async mounted() {
          const fromPromiseResult = ServiceResult.fromSubmitter<
            TestType,
            TestType
          >(() => Promise.reject<TestType>());

          expect(fromPromiseResult.submit({ data: "in" })).rejects.toBeFalsy();

          await flushPromises();

          //expect(y.state).toMatch("loading");
          //expect(y.success).toBeFalsy();

          //  x.catch(() => {
          expect(fromPromiseResult.state).toMatch("error");
          expect(fromPromiseResult.success).toBeFalsy();
        },
      }
    );

    //   });
  });
});
