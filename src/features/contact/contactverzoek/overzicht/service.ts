import {
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  parsePagination,
  type PaginatedResult,
} from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  fetchWithSysteemId,
  filterOutContactmomenten,
  KlantContactExpand,
  searchDigitaleAdressen,
  type Betrokkene,
  type BetrokkeneMetKlantContact,
  type ContactmomentViewModel,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import { enrichContactverzoekObjectWithContactmoment } from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./types";
import type { ContactmomentDetails } from "../../contactmoment";
import { fullName } from "@/helpers/string";

function searchRecursive(urlStr: string, page = 1): Promise<any[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (j) => {
      if (!Array.isArray(j?.results)) {
        throw new Error("Expected array: " + JSON.stringify(j));
      }

      if (!j.next) return j.results;
      const nextResults = await searchRecursive(urlStr, page + 1);
      return [...j.results, ...nextResults];
    });
}

async function searchOk2Recursive(
  adres: string,
  page = 1,
): Promise<DigitaalAdresExpandedApiViewModel[]> {
  const paginated = await searchDigitaleAdressen({
    adres,
    page,
    expand: [DigitaleAdressenExpand.verstrektDoorBetrokkene],
  });
  if (!paginated.next) return paginated.page;
  return [...paginated.page, ...(await searchOk2Recursive(adres, page + 1))];
}

export async function search(
  systeemId: string,
  query: string,
  gebruikKlantInteractiesApi: boolean,
): Promise<ContactverzoekOverzichtItem[]> {
  // OK2
  if (gebruikKlantInteractiesApi) {
    const adressen = await searchOk2Recursive(query);

    const betrokkenen = adressen
      .map((x) => x?._expand?.verstrektDoorBetrokkene)
      .filter(Boolean) as Betrokkene[];

    const uniqueBetrokkenen = new Map<string, Betrokkene>(
      betrokkenen.map((betrokkene) => [betrokkene.uuid, betrokkene]),
    );

    return enrichBetrokkeneWithKlantContact(
      systeemId,
      [...uniqueBetrokkenen.values()],
      [
        KlantContactExpand.leiddeTotInterneTaken,
        KlantContactExpand.gingOverOnderwerpobjecten,
      ],
    )
      .then(filterOutContactmomenten)
      .then((page) => enrichBetrokkeneWithDigitaleAdressen(systeemId, page))
      .then(enrichInterneTakenWithActoren)
      .then(mapKlantcontactToContactverzoekOverzichtItem)
      .then(filterOutGeauthenticeerdeContactverzoeken);
  }
  /// OK1 heeft geen interne taak, dus gaan we naar de objecten registratie
  else {
    const url = new URL("/api/internetaak/api/v2/objects", location.origin);
    url.searchParams.set("ordering", "-record__data__registratiedatum");
    url.searchParams.set("pageSize", "10");
    url.searchParams.set(
      "data_attr",
      `betrokkene__digitaleAdressen__icontains__${query}`,
    );

    return searchRecursive(url.toString())
      .then((x) =>
        Promise.all(x.map(enrichContactverzoekObjectWithContactmoment)),
      )
      .then((x) => x.map(mapObjectToContactverzoekOverzichtItem))
      .then(filterOutGeauthenticeerdeContactverzoeken);
  }
}

function filterOutGeauthenticeerdeContactverzoeken(
  value: ContactverzoekOverzichtItem[],
) {
  return value.filter((x) => !x.betrokkene?.isGeauthenticeerd);
}

function mapKlantcontactToContactverzoekOverzichtItem(
  betrokkeneMetKlantcontact: BetrokkeneMetKlantContact[],
): ContactverzoekOverzichtItem[] {
  return betrokkeneMetKlantcontact.map(
    ({ klantContact, contactnaam, expandedDigitaleAdressen, wasPartij }) => {
      const internetaak = klantContact._expand?.leiddeTotInterneTaken?.[0];
      if (!internetaak) {
        throw new Error("");
      }

      return {
        url: internetaak.url,
        onderwerp: klantContact.onderwerp,
        toelichtingBijContactmoment: klantContact.inhoud,
        status: internetaak.status,
        registratiedatum: klantContact.plaatsgevondenOp,
        vraag: klantContact.onderwerp,
        aangemaaktDoor: klantContact.hadBetrokkenActoren?.[0]?.naam || "",
        behandelaar: internetaak?.actor?.naam,
        toelichtingVoorCollega: internetaak.toelichting,
        betrokkene: {
          persoonsnaam: contactnaam,
          digitaleAdressen: expandedDigitaleAdressen || [],
          isGeauthenticeerd: !!wasPartij,
        },
        objecten:
          klantContact?._expand?.gingOverOnderwerpobjecten?.map((x) => ({
            object: x.onderwerpobjectidentificator.objectId,
            objectType: x.onderwerpobjectidentificator.codeObjecttype,
            contactmoment: x.klantcontact.uuid,
          })) || [],
      } satisfies ContactverzoekOverzichtItem;
    },
  );
}

function mapObjectToContactverzoekOverzichtItem({
  contactverzoekObject,
  contactmoment,
  details,
}: {
  contactverzoekObject: any;
  contactmoment: ContactmomentViewModel | null;
  details: ContactmomentDetails | null;
}): ContactverzoekOverzichtItem {
  const getVraag = (cd: ContactmomentDetails | null) => {
    const { vraag, specifiekeVraag } = cd || {};
    if (!vraag) return specifiekeVraag;
    if (!specifiekeVraag) return vraag;
    return `${vraag} (${specifiekeVraag})`;
  };
  const vraag = getVraag(details) || "";
  const record = contactverzoekObject.record;
  const data = record.data;

  return {
    url: contactverzoekObject.url,
    onderwerp: vraag,
    toelichtingBijContactmoment: contactmoment?.tekst || "",
    status: data.status || "onbekend",
    registratiedatum: data.registratiedatum,
    vraag,
    toelichtingVoorCollega: data.toelichting || "",
    behandelaar: data.actor?.naam || "",
    betrokkene: {
      isGeauthenticeerd: !!data.betrokkene?.klant,
      persoonsnaam: data.betrokkene?.persoonsnaam || {},
      digitaleAdressen: data.betrokkene?.digitaleAdressen || [],
    },
    aangemaaktDoor: fullName(contactmoment?.medewerkerIdentificatie),
    objecten: contactmoment?.objectcontactmomenten || [],
  } satisfies ContactverzoekOverzichtItem;
}

export function fetchContactverzoekenByKlantId(
  systeemId: string,
  id: string,
  gebruikKlantInteractiesApi: boolean,
): Promise<PaginatedResult<ContactverzoekOverzichtItem>> {
  //OK2
  if (gebruikKlantInteractiesApi) {
    return fetchBetrokkenen({
      systeemId: systeemId,
      pageSize: "100",
      wasPartij__url: id,
    }).then(async (paginated) => ({
      ...paginated,
      page: await enrichBetrokkeneWithKlantContact(systeemId, paginated.page, [
        KlantContactExpand.leiddeTotInterneTaken,
        KlantContactExpand.gingOverOnderwerpobjecten,
      ])
        .then(filterOutContactmomenten)
        .then((page) => enrichBetrokkeneWithDigitaleAdressen(systeemId, page))
        .then(enrichInterneTakenWithActoren)
        .then(mapKlantcontactToContactverzoekOverzichtItem),
    }));
  }

  // OK1
  const url = new URL("/api/internetaak/api/v2/objects", location.origin);
  url.searchParams.set("ordering", "-record__data__registratiedatum");
  url.searchParams.set("pageSize", "10");
  url.searchParams.set("data_attr", `betrokkene__klant__exact__${id}`);

  return fetchWithSysteemId(systeemId, url.toString())
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) =>
      parsePagination(r, (v) =>
        enrichContactverzoekObjectWithContactmoment(v).then(
          mapObjectToContactverzoekOverzichtItem,
        ),
      ),
    );
}
