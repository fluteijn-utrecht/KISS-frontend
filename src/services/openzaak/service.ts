import { throwIfNotOk } from "..";
import { fetchLoggedIn } from "../fetch-logged-in";
import type { ZaakContactmoment } from "./types";

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export const koppelZaakContactmoment = ({
  zaaksysteemId,
  url,
  contactmoment,
}: ZaakContactmoment) =>
  fetchLoggedIn(zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ zaaksysteemId, url, contactmoment }),
  }).then(throwIfNotOk);
