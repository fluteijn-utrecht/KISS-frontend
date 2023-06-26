import { flushPromises } from "@vue/test-utils";
import { fetchLoggedIn } from "@/services";

import { describe, expect, test } from "vitest";

import { setupServer } from "msw/node";
import { rest } from "msw";

describe("fetchLoggedIn", () => {
  ////////////////////////////////////////////////////////////
  //todo http request mocking generiek implementen ipv per test
  ////////////////////////////////////////////////////////////
  test("should resolve when a 200 response is returned", async () => {
    //http request mocking
    const restHandlers = [
      rest.get("https://dummy.request.fgdsf", (req, res, ctx) => {
        return res(ctx.status(200), ctx.json("test"));
      }),
    ];

    const server = setupServer(...restHandlers);

    // Start server before tests
    server.listen();

    try {
      fetchLoggedIn("https://dummy.request.fgdsf", {
        method: "GET",
      }).then((x: any) => {
        expect(x.status).toBe(200);
      });
      await flushPromises();
    } finally {
      //  Close server after tests
      server.close();

      // Reset handlers after each test `important for test isolation`
      server.resetHandlers();
    }
  });
});
