import { fetchLoggedIn, throwIfNotOk } from "@/services";
import type { Contactmoment } from "./types";

export type ContactmomentManagementInfo = Pick<
  Contactmoment,
  | "resultaat"
  | "startdatum"
  | "einddatum"
  | "primaireVraagWeergave"
  | "afwijkendOnderwerp"
>;

export const upsertContactmomentManagementInfo = (
  contactmoment: ContactmomentManagementInfo,
  id: string
): Promise<unknown> =>
  fetchLoggedIn("/api/managementinfo/contactmomenten", {
    method: "PUT",
    body: JSON.stringify({
      ...contactmoment,
      id,
    }),
    headers: { "content-type": "application/json" },
  }).then(throwIfNotOk);
