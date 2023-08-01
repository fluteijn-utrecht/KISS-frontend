import { formatIsoDate } from "@/helpers/date";
import type {
  GeslachtsnaamGeboortedatum,
  PostcodeHuisnummer,
} from "@/helpers/validation";
import {
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  ServiceResult,
  type ServiceData,
  enforceOneOrZero,
  defaultPagination,
  FriendlyError,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";
import type { Persoon } from "./types";

const zoekUrl = "/api/haalcentraal/brp/personen";

type QueryParam = [string, string | string[]][];

type SearchPersoonFieldParams = {
  bsn: string;
  geslachtsnaamGeboortedatum: GeslachtsnaamGeboortedatum;
  postcodeHuisnummer: PostcodeHuisnummer;
};

export type PersoonSearchField = keyof SearchPersoonFieldParams;

type PersoonQueryParams = {
  [K in PersoonSearchField]: (
    search: SearchPersoonFieldParams[K]
  ) => QueryParam;
};

export type PersoonQuery<K extends PersoonSearchField> = {
  field: K;
  value: SearchPersoonFieldParams[K];
};

export function persoonQuery<K extends PersoonSearchField>(
  args: PersoonQuery<K>
): PersoonQuery<K> {
  return args;
}

const minimalFields = [
  "burgerservicenummer",
  "geboorte.datum",
  "adressering.adresregel1",
  "adressering.adresregel2",
  "adressering.adresregel3",
  "adressering.land.omschrijving",
  "naam.voornamen",
  "naam.voorvoegsel",
  "naam.geslachtsnaam",
];

const queryDictionary: PersoonQueryParams = {
  bsn: (search) => [
    ["burgerservicenummer", [search]],
    ["type", "RaadpleegMetBurgerservicenummer"],
    ["fields", [...minimalFields, "geboorte.land", "geboorte.plaats"]],
  ],
  geslachtsnaamGeboortedatum: ({ geslachtsnaam, geboortedatum }) => [
    ["geboortedatum", formatIsoDate(geboortedatum)],
    [
      "geslachtsnaam",
      geslachtsnaam?.endsWith("*") ? geslachtsnaam : geslachtsnaam + "*",
    ],
    ["type", "ZoekMetGeslachtsnaamEnGeboortedatum"],
    ["fields", [...minimalFields]],
  ],
  postcodeHuisnummer: ({ postcode, huisnummer, toevoeging, huisletter }) => [
    ["postcode", `${postcode.numbers}${postcode.digits}`],
    ["huisnummer", huisnummer],
    ["huisnummertoevoeging", toevoeging || ""],
    ["huisletter", huisletter || ""],
    ["type", "ZoekMetPostcodeEnHuisnummer"],
    ["fields", [...minimalFields]],
  ],
};

function getQueryParams<K extends PersoonSearchField>(params: PersoonQuery<K>) {
  return queryDictionary[params.field](params.value) as ReturnType<
    PersoonQueryParams[K]
  >;
}

function mapPersoon(json: any): Persoon {
  const { adressering, naam, geboorte, burgerservicenummer } = json ?? {};
  const { plaats, land, datum } = geboorte ?? {};

  const { adresregel1, adresregel2, adresregel3 } = adressering ?? {};

  const { geslachtsnaam, voornamen, voorvoegsel } = naam ?? {};

  const geboortedatum = datum?.datum && new Date(datum.datum);

  return {
    _typeOfKlant: "persoon",
    bsn: burgerservicenummer,
    geboortedatum,
    voornaam: voornamen,
    voorvoegselAchternaam: voorvoegsel,
    achternaam: geslachtsnaam,
    geboorteplaats: plaats?.omschrijving,
    geboorteland: land?.omschrijving,
    adresregel1,
    adresregel2,
    adresregel3,
  };
}

function getPersoonUniqueBsnId(bsn: string | undefined) {
  return bsn ? zoekUrl + "_single" + bsn : "";
}

const searchSinglePersoon = (bsn: string) =>
  searchPersonen({
    field: "bsn",
    value: bsn,
  }).then(enforceOneOrZero);

export const searchPersonen = <K extends PersoonSearchField>(
  query: PersoonQuery<K>
) => {
  const entries = getQueryParams(query);
  const body = JSON.stringify(Object.fromEntries(entries));
  return fetchLoggedIn(zoekUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body,
  })
    .then(async (r) => {
      const contentType = r.headers.get("content-type");
      if (
        r.status === 400 &&
        contentType?.match(/^application\/problem\+json.*/)?.[0]
      ) {
        const json = await r.json();
        if (json?.code && json?.detail) {
          throw new FriendlyError(json.detail);
        }
      }
      throwIfNotOk(r);
      return r.json();
    })
    .then((json) => {
      const mapped: Persoon[] = [];
      json.personen.forEach((p: any) => {
        const persoon = mapPersoon(p);
        const key = getPersoonUniqueBsnId(persoon.bsn);
        if (key) {
          mutate(key, persoon);
        }
        mapped.push(persoon);
      });
      return defaultPagination(mapped);
    });
};

export function usePersoonByBsn(
  getBsn: () => string | undefined
): ServiceData<Persoon | null> {
  return ServiceResult.fromFetcher(
    zoekUrl,
    () => searchSinglePersoon(getBsn() || ""),
    {
      getUniqueId: () => getPersoonUniqueBsnId(getBsn()),
    }
  );
}

type UseSearchParams<K extends PersoonSearchField> = {
  query: Ref<PersoonQuery<K> | undefined>;
};

export function useSearchPersonen<K extends PersoonSearchField>({
  query,
}: UseSearchParams<K>) {
  return ServiceResult.fromFetcher(
    zoekUrl,
    () => {
      if (!query.value) {
        throw new Error("should not happen, already checked in unique id");
      }
      return searchPersonen(query.value);
    },
    {
      getUniqueId() {
        return query.value ? JSON.stringify(query.value) : "";
      },
    }
  );
}
