import { flushPromises } from "@vue/test-utils";
import { combineEnrichers, ServiceResult, type ServiceData } from "@/services";

import {
  assertType,
  describe,
  expect,
  expectTypeOf,
  test,
  type Test,
} from "vitest";

describe("service-data-enricher-test", () => {
  test("service-data-enricher of type Left, Not having the necessary key to retrieve data of type Right, should ... ", async () => {
    type LeftType = {
      _typeOf: "left";
      dataLeftOnly: string;
      dataBoth: string;
    };

    type RightType = {
      _typeOf: "right";
      dataRightOnly: string;
      dataBoth: string;
    };

    function left(): ServiceData<LeftType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "both",
          dataLeftOnly: "left",
          _typeOf: "left",
        })
      );
    }

    function right(): ServiceData<RightType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "both",
          dataRightOnly: "right",
          _typeOf: "right",
        })
      );
    }

    const isLeft = (
      leftOrRight: LeftType | RightType
    ): leftOrRight is LeftType => {
      return leftOrRight._typeOf === "left";
    };

    const testEnricher = combineEnrichers(
      left,
      right,
      (either) => either.dataBoth, //wordt als parameter meegegeven bij het ophalen van het andere object
      isLeft
    );

    const [common, leftData, rightData] = testEnricher(() => {
      return {
        _typeOf: "left",
        dataLeftOnly: "leftonly",
        dataBoth: "bothLeftAndRight",
      };
    });

    expect(common.value).toMatch("bothLeftAndRight");
    //leftData.success
    expect(leftData).toMatch("sss");
    //  expect(rightData).toMatch("sss");
  });
});
