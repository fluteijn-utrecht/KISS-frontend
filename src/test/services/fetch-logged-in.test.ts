//TODO fix test
// import { flushPromises } from "@vue/test-utils";
// import { fetchLoggedIn, handleLogin } from "@/services";

// import { describe, expect, test, afterAll, vi } from "vitest";

// import { setupServer } from "msw/node";
// import { rest } from "msw";

// describe("fetchLoggedIn", () => {
//   const consoleMock = vi.spyOn(console, "log").mockImplementation(() => {});

//   afterAll(() => {
//     consoleMock.mockReset();
//   });

//   ////////////////////////////////////////////////////////////
//   //todo http request mocking generiek implementen ipv per test
//   ////////////////////////////////////////////////////////////
//   test("should resolve when a 200 response is returned", async () => {
//     //http request mocking
//     const restHandlers = [
//       rest.get("https://dummy.request.fgdsf", (req, res, ctx) => {
//         return res(ctx.status(200), ctx.json("test"));
//       }),
//     ];

//     const server = setupServer(...restHandlers);

//     // Start server before tests
//     server.listen();

//     try {
//       fetchLoggedIn("https://dummy.request.fgdsf", {
//         method: "GET",
//       }).then((x: any) => {
//         expect(x.status).toBe(200);
//       });
//       await flushPromises();
//     } finally {
//       //  Close server after tests
//       server.close();

//       // Reset handlers after each test `important for test isolation`
//       server.resetHandlers();
//     }
//   });

//   test("should ... when a 401 response is returned", async () => {
//     let unauthcallsCount = 0;
//     let successcallsCount = 0;

//     const restHandlers = [
//       rest.get("https://dummy.request.fgdsf", (req, res, ctx) => {
//         return res(ctx.status(401), ctx.json("test"));
//       }),
//     ];

//     const server = setupServer(...restHandlers);

//     // Start server before tests
//     server.listen();

//     const restHandlers2 = [
//       rest.get("https://dummy.request.fgdsf", (req, res, ctx) => {
//         return res(ctx.status(200), ctx.json("test"));
//       }),
//     ];

//     const server2 = setupServer(...restHandlers2);

//     try {
//       //http request mocking
//       fetchLoggedIn("https://dummy.request.fgdsf", {
//         method: "GET",
//       }).then((x: any) => {
//         if (x.status == 200) {
//           successcallsCount++;
//           expect(consoleMock).toHaveBeenLastCalledWith(
//             "user is logged in again, resuming..."
//           );

//           handleLogin();

//           //  Close server after tests
//           server.close();

//           // Reset handlers after each test `important for test isolation`
//           server.resetHandlers();

//           // Start server before tests
//           server2.listen();

//           return;
//         }

//         if (x.status == 401) {
//           unauthcallsCount++;
//           expect(consoleMock).toHaveBeenLastCalledWith(
//             "session expired. waiting for user to log in..."
//           );
//           return;
//         }
//       });

//       //  await flushPromises();

//       await flushPromises();
//     } finally {
//       //  Close server after tests
//       server.close();

//       // Reset handlers after each test `important for test isolation`
//       server.resetHandlers();

//       //  Close server after tests
//       server2.close();

//       // Reset handlers after each test `important for test isolation`
//       server2.resetHandlers();

//       expect(successcallsCount).toBe(1);
//       expect(unauthcallsCount).toBe(1);
//     }
//   });
// });
