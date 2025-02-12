import { throwIfNotOk, parseJson, fetchLoggedIn } from "..";

type ValueOf<T> = T[keyof T];

export const registryVersions = {
  ok1: "OpenKlant1",
  ok2: "OpenKlant2",
} as const;

export const fetchSystemen = () =>
  fetchLoggedIn("/api/environment/registers")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(
      ({ systemen }) =>
        systemen as {
          isDefault: boolean;
          identifier: string;
          registryVersion: ValueOf<typeof registryVersions>;
        }[],
    );
