import { throwIfNotOk, parseJson, fetchLoggedIn } from "..";

type ValueOf<T> = T[keyof T];

export const registryVersions = {
  ok1: "OpenKlant1",
  ok2: "OpenKlant2",
} as const;

export type Systeem = {
  isDefault: boolean;
  identifier: string;
  registryVersion: ValueOf<typeof registryVersions>;
};

export const fetchSystemen = () =>
  fetchLoggedIn("/api/environment/registers")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ systemen }) => systemen as Systeem[]);
