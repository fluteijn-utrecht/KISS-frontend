import { DateTime } from "luxon";
import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type {
  Medewerker,
  NatuurlijkPersoon,
  NietNatuurlijkPersoon,
  OrganisatorischeEenheid,
  RolType,
  Vestiging,
  ZaakDetails,
  ZaakDocument,
  ZaakType,
} from "./types";
import type { Ref } from "vue";
import { mutate } from "swrv";

export const useZakenByBsn = (bsn: Ref<string>) => {
  const getUrl = () => {
    if (!bsn.value) return "";
    const url = new URL(location.href);
    url.pathname = zaaksysteemBaseUri + "/zaken";
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

export const useZakenByVestigingsnummer = (vestigingsnummer: Ref<string>) => {
  const getUrl = () => {
    if (!vestigingsnummer.value) return "";
    const url = new URL(location.href);
    url.pathname = zaaksysteemBaseUri + "/zaken";
    url.searchParams.set(
      "embedded.rollen.embedded.betrokkeneIdentificatie.vestigingsNummer",
      vestigingsnummer.value
    );
    return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

const getNamePerRoltype = (rollen: Array<RolType> | null, roleNaam: string) => {
  const ONBEKEND = "Onbekend";

  if (!rollen) {
    return ONBEKEND;
  }

  //we gaan er in de interface vanuit dat een rol maar 1 keer voorkomt bij een zaak
  const rol = rollen.find(
    (rol: RolType) => rol.omschrijvingGeneriek === roleNaam
  );

  if (!rol || !rol.betrokkeneIdentificatie) {
    return ONBEKEND;
  }

  if (rol.betrokkeneType === "natuurlijk_persoon") {
    const x = rol.betrokkeneIdentificatie as NatuurlijkPersoon;
    return [x.voornamen, x.voorvoegselGeslachtsnaam, x.geslachtsnaam]
      .filter(Boolean)
      .join(" ");
  } else if (rol.betrokkeneType === "niet_natuurlijk_persoon") {
    const x = rol.betrokkeneIdentificatie as NietNatuurlijkPersoon;
    return x.statutaireNaam;
  } else if (rol.betrokkeneType === "vestiging") {
    const x = rol.betrokkeneIdentificatie as Vestiging;
    return [x.naam, x.vestigingsNummer].filter(Boolean).join(" ");
  } else if (rol.betrokkeneType === "organisatorische_eenheid") {
    const x = rol.betrokkeneIdentificatie as OrganisatorischeEenheid;
    return x.naam;
  } else if (rol.betrokkeneType === "medewerker") {
    const x = rol.betrokkeneIdentificatie as Medewerker;
    return [x.voorletters, x.voorvoegselAchternaam, x.achternaam]
      .filter(Boolean)
      .join(" ");
  }
  //

  return ONBEKEND;
};

const getStatus = async (statusUrl: string) => {
  const statusId = statusUrl?.split("/")?.pop();

  if (!statusId) return "";

  const statusType = await fetchLoggedIn(
    `${zaaksysteemBaseUri}/statussen/${statusId}`
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.statustype);

  const statusTypeUuid = statusType?.split("/")?.pop();

  if (!statusTypeUuid) return "";

  const statusTypeUrl = `/api/zaken/catalogi/api/v1/statustypen/${statusTypeUuid}`;

  const statusOmschrijving = await fetchLoggedIn(statusTypeUrl)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.omschrijving);

  return statusOmschrijving;
};

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

const getRollen = async (zaakurl: string): Promise<Array<RolType>> => {
  // rollen is een gepagineerd resultaat. we verwachten maar twee rollen.
  // het lijkt extreem onwaarschijnlijk dat er meer dan 1 pagina met rollen zal zijn.
  // we kijken dus (voorlopig) alleen naar de eerste pagina

  let pageIndex = 0;
  const rollen: Array<RolType> = [];
  const rollenUrl = `${zaaksysteemBaseUri}/rollen?zaak=${zaakurl}`;

  const getPage = async (url: string) => {
    const page = await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((x) => x.json())
      .then((json) => parsePagination(json, async (x: any) => x as RolType));

    rollen.push(...page.page);
    if (page.next) {
      pageIndex++;
      const nextUrl = `${rollenUrl}&page=${pageIndex}`;
      await getPage(nextUrl);
    }
  };

  await getPage(rollenUrl);

  return rollen;
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

const getZaakUrl = (id: string) => {
  if (!id) return "";
  return `${zaaksysteemBaseUri}/zaken/${id}`;
};

const mapZaakDetails = async (zaak: any) => {
  const zaakzaaktype = await getZaakType(zaak.zaaktype);

  const startdatum = zaak.startdatum ? new Date(zaak.startdatum) : undefined;

  // voorlopig disabled: openzaakbrondata is niet conform de standaard
  // const doorlooptijd = parseInt(
  //   zaakzaaktype.doorlooptijd
  //     ? zaakzaaktype.doorlooptijd.replace(/^\D+/g, "")
  //     : "0",
  //   10
  // );

  // const servicenorm = parseInt(
  //   zaakzaaktype.servicenorm
  //     ? zaakzaaktype.servicenorm.replace(/^\D+/g, "")
  //     : "0",
  //   10
  // );

  // const fataleDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: isNaN(doorlooptijd) ? 0 : doorlooptijd,
  //     })
  //     .toJSDate();

  // const streefDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: isNaN(servicenorm) ? 0 : servicenorm,
  //     })
  //     .toJSDate();

  const documenten = await getDocumenten(zaak.url);

  const rollen = await getRollen(zaak.url);

  const id = zaak.url.split("/").pop();

  return {
    ...zaak,
    id: id,
    zaaktype: zaakzaaktype.id,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    zaaktypeOmschrijving: zaakzaaktype.omschrijving,
    status: await getStatus(zaak.status),
    behandelaar: getNamePerRoltype(rollen, "behandelaar"),
    aanvrager: getNamePerRoltype(rollen, "initiator"),
    startdatum,
    // fataleDatum: fataleDatum, voorlopig niet tonen: openzaakbrondata is niet conform de standaard
    // streefDatum: streefDatum, voorlopig niet tonen: openzaakbrondata is niet conform de standaard
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
    downloadUrl: xx + "/download?versie=1",
    //werkt nog niet, wordt in een volgende story oafgemaakt
    // downloadUrl:
    // "/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1",

    // "/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/adcdddd9-3d90-4488-b7c8-96ff017195a9/download?versie=1",
    // "https://open-zaak.dev.kiss-demo.nl/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1",

    // https://localhost:3000/api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/adcdddd9-3d90-4488-b7c8-96ff017195a9/download?versie=1

    // rawDocumenten.inhoud?.split("/").pop(), //https://open-zaak.dev.kiss-demo.nl/documenten/api/v1/enkelvoudiginformatieobjecten/c733e749-2dc5-4d29-a45f-165094e21d6f/download?versie=1
  };
  return doc;
};
