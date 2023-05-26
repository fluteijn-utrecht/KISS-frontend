import { DateTime } from "luxon";
import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
  type PaginatedResult,
} from "@/services";
import type { ZaakDetails, ZaakType } from "./types";
import type { Ref } from "vue";
import { mutate } from "swrv";
import { formatIsoDate } from "@/helpers/date";

type Roltype = "behandelaar" | "initiator";
const ONBEKEND = "Onbekend";

const getNamePerRoltype = (zaak: any, roletype: Roltype) => {
  if (!Array.isArray(zaak?.embedded?.rollen)) return ONBEKEND;
  const rolesArr = typeof roletype === "string" ? [roletype] : roletype;
  const behandelaar = zaak.embedded.rollen.find((rol: any) =>
    rolesArr.includes(rol?.embedded?.roltype?.omschrijvingGeneriek)
  );
  const { voorletters, voornamen, voorvoegselGeslachtsnaam, geslachtsnaam } =
    behandelaar?.embedded?.betrokkeneIdentificatie ?? {};

  const name = [
    voornamen || voorletters,
    voorvoegselGeslachtsnaam,
    geslachtsnaam,
  ]
    .filter(Boolean)
    .join(" ");

  return name || ONBEKEND;
};

const mapZaakDetails = async (zaak: any) => {
  console.log("----", zaak);

  const zaakzaaktype = await getZaakType(zaak.zaaktype);

  const startdatum = zaak.startdatum ? new Date(zaak.startdatum) : undefined;

  //temp. todo fix
  const fataleDatum = new Date();
  const streefDatum = new Date();
  // const fataleDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: parseInt(zaakzaaktype.doorlooptijd, 10),
  //     })
  //     .toJSDate();

  // const streefDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: parseInt(zaakzaaktype.servicenorm, 10),
  //     })
  //     .toJSDate();

  return {
    ...zaak,
    zaaktype: zaakzaaktype.id,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    zaaktypeOmschrijving: zaakzaaktype.omschrijving,
    status: zaak.status.statustoelichting,
    behandelaar: getNamePerRoltype(zaak, "behandelaar"),
    aanvrager: getNamePerRoltype(zaak, "initiator"),
    startdatum,
    fataleDatum: fataleDatum,
    streefDatum: streefDatum,
    indienDatum: zaak.publicatiedatum && new Date(zaak.publicatiedatum),
    registratieDatum: zaak.registratiedatum && new Date(zaak.registratiedatum),
    self: zaak.url,
    documenten: mapDocumenten(zaak?.embedded?.zaakinformatieobjecten),
    omschrijving: zaak.omschrijving,
  } as ZaakDetails;
};
//zaken/api/v1/
const zaaksysteemBaseUri = `/api/zaken/zaken/api/v1/zaken`;
//const zaaksysteemBaseUri = `/api/zaken/zaken/api/v1/zaken`;

const overviewFetcher = (url: string): Promise<PaginatedResult<ZaakDetails>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => parsePagination(json, mapZaakDetails))
    .then((zaken) => {
      zaken.page.forEach((zaak) => {
        console.log("--zaak:", zaak);

        mutate(getZaakUrl(zaak.id), zaak);
      });
      return zaken;
    });

const getZaakType = (zaaktype: string): Promise<ZaakType> => {
  const zaaktypeid = zaaktype.split("/").pop();
  const url = `/api/zaken/catalogi/api/v1/zaaktypen/${zaaktypeid}`;

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => {
      console.log("zaaktype: ", json);

      return json;
    });
};

export const useZakenByBsn = (bsn: Ref<string>) => {
  const getUrl = () => {
    if (!bsn.value) return "";
    const url = new URL(zaaksysteemBaseUri);
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
    return zaaksysteemBaseUri;
    //if (!zaaknummer.value) return "";
    //const url = new URL(zaaksysteemBaseUri);
    //url.searchParams.set("identificatie", zaaknummer.value);
    //return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

const getZaakUrl = (id: string) => {
  if (!id) return "";
  const url = new URL(`${zaaksysteemBaseUri}/${id}`);
  return url.toString();
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

const mapDocumenten = (rawDocumenten: any[]) => {
  if (!rawDocumenten) return [];

  return rawDocumenten.map((document) => ({
    id: document.embedded.informatieobject.id,
    titel: document.embedded.informatieobject.titel,
    bestandsomvang: document.embedded.informatieobject.bestandsomvang,
    creatiedatum: new Date(document.embedded.informatieobject.creatiedatum),
    vertrouwelijkheidaanduiding:
      document.embedded.informatieobject.vertrouwelijkheidaanduiding,
    formaat: document.embedded.informatieobject.formaat,
    inhoud: document.embedded.informatieobject.inhoud,
  }));
};

export const useZakenByVestigingsnummer = (vestigingsnummer: Ref<string>) => {
  const getUrl = () => {
    if (!vestigingsnummer.value) return "";
    const url = new URL(zaaksysteemBaseUri);
    url.searchParams.set(
      "embedded.rollen.embedded.betrokkeneIdentificatie.vestigingsNummer",
      vestigingsnummer.value
    );
    return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};
