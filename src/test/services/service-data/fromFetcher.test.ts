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
  test("ServiceResult.fromFetcher: the correct result is returned for a resolved Promise.", async () => {
    type TestType = {
      data: string;
    };

    //de te testen code is gewrapped in een leeg vue component en wordt aangeroepen vanuit de mounted lifecycle hook
    //de reden hiervoor is dat fromFetcher gebruik maakt van een SWRV caching lib, die allaen correct functioneert vanuit de context van een vue component
    mount(
      {},
      {
        async mounted() {
          const fromPromiseResult = ServiceResult.fromFetcher<TestType>(
            "dummyurl",
            () => Promise.resolve({ data: "value" } as TestType)
          );

          expectTypeOf(fromPromiseResult).toMatchTypeOf<
            ServiceData<TestType>
          >();

          await flushPromises();

          expect(fromPromiseResult.state).toMatch("success");
          expect(fromPromiseResult.success).toBeTruthy();
        },
      }
    );
  });

  test("ServiceResult.fromPromise: test if the correct result is returned for a pending Promise.", async () => {
    type TestType = {
      data: string;
    };

    //de te testen code is gewrapped in een leeg vue component en wordt aangeroepen vanuit de mounted lifecycle hook
    //de reden hiervoor is dat fromFetcher gebruik maakt van een SWRV caching lib, die allaen correct functioneert vanuit de context van een vue component
    mount(
      {},
      {
        async mounted() {
          const fromPromiseResult = ServiceResult.fromFetcher<TestType>(
            "dummyurl",
            () => Promise.resolve({ data: "value" } as TestType)
          );

          expectTypeOf(fromPromiseResult).toMatchTypeOf<
            ServiceData<TestType>
          >();

          // promise wordt niet geflushed en blijft daardoor pending?

          expect(fromPromiseResult.state).toMatch("loading");
          expect(fromPromiseResult.success).toBeFalsy();
        },
      }
    );
  });

  //nb. testen met een rejected promise kan niet icm useSWRV
});
