import {
  fetchLoggedIn,
  parseJson,
  throwIfNotOk,
  type Paginated,
  defaultPagination,
  FriendlyError,
} from "@/services";

import type { Bedrijf } from "../../types";

export const zoekenUrl = "/api/kvk/v2/zoeken";
const vestigingsprofielenUrl = "/api/kvk/v1/vestigingsprofielen/";

type KvkVestiging = {
  vestigingsnummer: string;
  kvkNummer: string;
  handelsnaam: string;
  postcode: string;
  huisnummer: string;
  huisletter: string;
  huisnummertoevoeging: string;
};

export function searchBedrijvenInHandelsRegister(url: string) {
  return fetchLoggedIn(url).then(async (r) => {
    if (r.status === 404) {
      const body = await r.json();
      if (hasFoutCode(body, "IPD5200")) return defaultPagination([]);
    }
    if (r.status === 400) {
      throw new FriendlyError("Invalide zoekopdracht");
    }
    throwIfNotOk(r);
    const body = await r.json();
    return parseKvkPagination(body);
  });
}

const parseKvkPagination = async ({
  pagina,
  resultatenPerPagina,
  totaal,
  resultaten,
}: KvkPagination): Promise<Paginated<Bedrijf>> => ({
  page: await Promise.all(resultaten.map(mapHandelsRegister)),
  pageNumber: pagina,
  totalRecords: totaal,
  pageSize: resultatenPerPagina,
  totalPages: totaal === 0 ? 0 : Math.ceil(totaal / resultatenPerPagina),
});

type KvkPagination = {
  pagina: number;
  resultatenPerPagina: number;
  totaal: number;
  resultaten: any[];
};

async function mapHandelsRegister(json: any): Promise<Bedrijf> {
  const { vestigingsnummer, kvkNummer, naam, adres } = json ?? {};

  const { binnenlandsAdres, buitenlandsAdres } = adres ?? {};

  const { straatnaam, plaats } = binnenlandsAdres ?? {};

  const { straatHuisnummer, postcodeWoonplaats } = buitenlandsAdres ?? {};

  let vestiging: KvkVestiging | undefined;

  if (vestigingsnummer) {
    try {
      vestiging = await fetchVestiging(getVestingUrl(vestigingsnummer));
    } catch (e) {
      console.error(e);
    }
  }

  return {
    _typeOfKlant: "bedrijf",
    kvkNummer,
    vestigingsnummer,
    bedrijfsnaam: naam,
    straatnaam: straatnaam || straatHuisnummer,
    woonplaats: plaats || postcodeWoonplaats,
    ...(vestiging ?? {}),
  };
}

const fetchVestiging = (url: string) =>
  fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapVestiging);

const getVestingUrl = (vestigingsnummer?: string) => {
  if (!vestigingsnummer) return "";
  return vestigingsprofielenUrl + vestigingsnummer;
};

const mapVestiging = ({
  vestigingsnummer,
  kvkNummer,
  eersteHandelsnaam,
  adressen,
}: any): KvkVestiging => {
  const { huisnummerToevoeging, postcode, huisnummer, huisletter } =
    adressen?.find((x: any) => x?.type === "bezoekadres") ??
    adressen?.[0] ??
    {};

  return {
    vestigingsnummer,
    kvkNummer,
    handelsnaam: eersteHandelsnaam,
    huisnummertoevoeging: huisnummerToevoeging,
    postcode,
    huisnummer,
    huisletter,
  };
};

const hasFoutCode = (body: unknown, code: string) => {
  if (
    body &&
    typeof body === "object" &&
    "fout" in body &&
    Array.isArray(body.fout)
  ) {
    return body.fout.some((x) => x?.code === code);
  }
  return false;
};
