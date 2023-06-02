import { DateTime } from "luxon";
import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type { RolType, ZaakDetails, ZaakDocument, ZaakType } from "./types";
import type { Ref } from "vue";
import { mutate } from "swrv";

const getNamePerRoltype = (rollen: Array<RolType> | null, roleNaam: string) => {
  const ONBEKEND = "Onbekend";

  if (!rollen) {
    return ONBEKEND;
  }

  const rol = rollen.find(
    (rol: RolType) => rol.omschrijvingGeneriek === roleNaam
  );

  if (!rol) {
    return ONBEKEND;
  }

  const { voorletters, voorvoegselAchternaam, achternaam, geslachtsnaam } =
    rol.betrokkeneIdentificatie ?? {};

  const name = [voorletters, voorvoegselAchternaam, achternaam || geslachtsnaam]
    .filter(Boolean)
    .join(" ");

  return name || ONBEKEND;
};

const mapZaakDetails = async (zaak: any) => {
  const zaakzaaktype = await getZaakType(zaak.zaaktype);

  const startdatum = zaak.startdatum ? new Date(zaak.startdatum) : undefined;

  const doorlooptijd = parseInt(
    zaakzaaktype.doorlooptijd
      ? zaakzaaktype.doorlooptijd.replace(/^\D+/g, "")
      : "0",
    10
  );

  const servicenorm = parseInt(
    zaakzaaktype.servicenorm
      ? zaakzaaktype.servicenorm.replace(/^\D+/g, "")
      : "0",
    10
  );

  const fataleDatum =
    startdatum &&
    DateTime.fromJSDate(startdatum)
      .plus({
        days: isNaN(doorlooptijd) ? 0 : doorlooptijd,
      })
      .toJSDate();

  const streefDatum =
    startdatum &&
    DateTime.fromJSDate(startdatum)
      .plus({
        days: isNaN(servicenorm) ? 0 : servicenorm,
      })
      .toJSDate();

  const documenten = await getDocumenten(zaak.url);

  const rollen = await getRollen(zaak.url);

  const id = zaak.url.split("/").pop();

  return {
    ...zaak,
    id: id,
    zaaktype: zaakzaaktype.id,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    zaaktypeOmschrijving: zaakzaaktype.omschrijving,
    status: zaak.status.statustoelichting,
    behandelaar: getNamePerRoltype(rollen, "behandelaar"),
    aanvrager: getNamePerRoltype(rollen, "initiator"),
    startdatum,
    fataleDatum: fataleDatum,
    streefDatum: streefDatum,
    indienDatum: zaak.publicatiedatum && new Date(zaak.publicatiedatum),
    registratieDatum: zaak.registratiedatum && new Date(zaak.registratiedatum),
    self: zaak.url,
    documenten: documenten,
    omschrijving: zaak.omschrijving,
  } as ZaakDetails;
};

const zaaksysteemBaseUri = `/api/zaken/zaken/api/v1`;
const documentenBaseUri = `/api/documenten/documenten/api/v1`;

const overviewFetcher = (url: string): Promise<PaginatedResult<ZaakDetails>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => parsePagination(json, mapZaakDetails))
    .then((zaken) => {
      zaken.page.forEach((zaak) => {
        mutate(getZaakUrl(zaak.id), zaak);
      });
      return zaken;
    });

const getDocumenten = async (
  zaakurl: string
): Promise<Array<ZaakDocument | null>> => {
  const infoObjecten = await fetchLoggedIn(
    `${zaaksysteemBaseUri}/zaakinformatieobjecten?zaak=${zaakurl}`
  )
    .then(throwIfNotOk)
    .then((x) => x.json());

  if (Array.isArray(infoObjecten)) {
    const promises = infoObjecten.map(async (item: any) => {
      const id = item.informatieobject.split("/").pop();

      const docUrl = `${documentenBaseUri}/enkelvoudiginformatieobjecten/${id}`;
      return fetchLoggedIn(docUrl)
        .then(throwIfNotOk) //todo 404 afvanengen?
        .then((x) => x.json())
        .then((x) => mapDocument(x, docUrl));
    });

    return await Promise.all(promises);
  }

  return [];
};

const mapRol = async (rol: any) => {
  return {
    ...rol,
  } as RolType;
};

const getRollen = async (zaakurl: string): Promise<Array<RolType>> => {
  // rollen is een gepagineerd resultaat. we verwachten maar twee rollen.
  // het lijkt extreem onwaarschijnlijk dat er meer dan 1 pagina met rollen zal zijn.
  // we kijken dus (voorlopig) alleen naar de eerste pagina

  const rollen = await fetchLoggedIn(
    `${zaaksysteemBaseUri}/rollen?zaak=${zaakurl}`
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => parsePagination(json, mapRol))
    .then((rollen) => {
      return rollen;
    });

  return rollen?.page;
};

const getZaakType = (zaaktype: string): Promise<ZaakType> => {
  const zaaktypeid = zaaktype.split("/").pop();
  const url = `/api/zaken/catalogi/api/v1/zaaktypen/${zaaktypeid}`;

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => {
      return json;
    });
};

export const useZakenByBsn = (bsn: Ref<string>) => {
  const getUrl = () => {
    if (!bsn.value) return "";
    const url = new URL(zaaksysteemBaseUri + "/zaken");
    url.searchParams.set(
      "embedded.rollen.embedded.betrokkeneIdentificatie.inpBsn",
      bsn.value
    );
    return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

export const useZakenByZaaknummer = (zaaknummer: Ref<string>) => {
  const getUrl = () => {
    if (!zaaknummer.value) return "";
    return `${zaaksysteemBaseUri}/zaken?identificatie=${zaaknummer.value}`;
  };
  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

const getZaakUrl = (id: string) => {
  if (!id) return "";
  return `${zaaksysteemBaseUri}/zaken/${id}`;
};

export const useZaakById = (id: Ref<string>) => {
  const getUrl = () => getZaakUrl(id.value);

  function fetcher(url: string): Promise<ZaakDetails> {
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((x) => x.json())
      .then(mapZaakDetails);
  }

  return ServiceResult.fromFetcher(getUrl, fetcher);
};

export async function updateToelichting(
  zaak: ZaakDetails,
  toelichting: string
): Promise<void> {
  const url = getZaakUrl(zaak.id);
  const res = await fetchLoggedIn(url, {
    method: "PATCH",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      toelichting: toelichting,
    }),
  });

  if (!res.ok)
    throw new Error(`Expected to update toelichting: ${res.status.toString()}`);

  const json = await res.json();
  const updatedZaak = mapZaakDetails(json);
  mutate(url, updatedZaak);
}

const mapDocument = (rawDocumenten: any, xx: string): ZaakDocument | null => {
  if (!rawDocumenten) return null;

  const doc = {
    id: rawDocumenten.identificatie,
    titel: rawDocumenten.titel,
    bestandsomvang: rawDocumenten.bestandsomvang,
    creatiedatum: new Date(rawDocumenten.creatiedatum),
    vertrouwelijkheidaanduiding: rawDocumenten.vertrouwelijkheidaanduiding,
    formaat: rawDocumenten.formaat,
    downloadUrl: xx + "/download?versie=1", //werkt nog niet
    // downloadUrl:
    // "/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1",

    // "/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/adcdddd9-3d90-4488-b7c8-96ff017195a9/download?versie=1",
    // "https://open-zaak.dev.kiss-demo.nl/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1",

    // https://localhost:3000/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/adcdddd9-3d90-4488-b7c8-96ff017195a9/download?versie=1

    // rawDocumenten.inhoud?.split("/").pop(), //https://open-zaak.dev.kiss-demo.nl/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1
  };
  return doc;
};

export const useZakenByVestigingsnummer = (vestigingsnummer: Ref<string>) => {
  const getUrl = () => {
    if (!vestigingsnummer.value) return "";
    const url = new URL(zaaksysteemBaseUri + "/zaken");
    url.searchParams.set(
      "embedded.rollen.embedded.betrokkeneIdentificatie.vestigingsNummer",
      vestigingsnummer.value
    );
    return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};
