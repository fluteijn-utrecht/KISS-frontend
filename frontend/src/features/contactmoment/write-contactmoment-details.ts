import { fetchLoggedIn, throwIfNotOk } from "@/services";
import type { Contactmoment } from "./types";

export type ContactmomentDetails = Pick<
  Contactmoment,
  | "gespreksresultaat"
  | "startdatum"
  | "einddatum"
  | "vraag"
  | "specifiekevraag"
>;

export const writeContactmomentDetails = (
  contactmoment: ContactmomentDetails,
  id: string
): Promise<unknown> =>
    fetchLoggedIn("/api/contactmomentdetails", {
    method: "PUT",
    body: JSON.stringify({
      ...contactmoment,
      id,
    }),
    headers: { "content-type": "application/json" },
  }).then(throwIfNotOk);
