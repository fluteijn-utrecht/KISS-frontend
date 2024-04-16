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
  const { vestigingsnummer, kvkNummer, naam, adres, type } = json ?? {};

  const { binnenlandsAdres, buitenlandsAdres } = adres ?? {};

  const { straatnaam, plaats } = binnenlandsAdres ?? {};

  const { straatHuisnummer, postcodeWoonplaats } = buitenlandsAdres ?? {};

  let vestiging: KvkVestiging | undefined;
  let naamgeving: KvkNaamgeving | undefined;

  if (vestigingsnummer) {
    try {
      vestiging = await fetchVestiging(getVestingUrl(vestigingsnummer));
    } catch (e) {
      console.error(e);
    }
  } else {
    // als er geen verstiging is dan gaan we ervan uit dat het een
    // niet natuurlijk persoon betreft waarvan we de RSIN proberen te achterhalen
    try {
      naamgeving = await fetchNaamgevingen(getNaamgevingenUrl(kvkNummer));
    } catch (e) {
      console.error(e);
    }
  }

  return {
    _typeOfKlant: "bedrijf",
    type,
    kvkNummer,
    vestigingsnummer,
    bedrijfsnaam: naam,
    straatnaam: straatnaam || straatHuisnummer,
    woonplaats: plaats || postcodeWoonplaats,
    ...(vestiging ?? {}),
    ...(naamgeving ?? {}),
  };
}

////

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

//// KvK naamgevingen //////////////////////////////////////////

const naamgevingenUrl = "/api/kvk/v1/naamgevingen/kvknummer/";

type KvkNaamgeving = {
  rsin: string;
  kvkNummer: string;
  handelsnaam: string;
};

const fetchNaamgevingen = (url: string) =>
  fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapNaamgeving);

const getNaamgevingenUrl = (kvkNummer?: string) => {
  if (!kvkNummer) return "";
  return naamgevingenUrl + kvkNummer;
};

const mapNaamgeving = ({ rsin, kvkNummer, naam }: any): KvkNaamgeving => {
  return {
    rsin,
    kvkNummer,
    handelsnaam: naam,
  };
};

////

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
