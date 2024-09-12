import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

import type {
  ContactmomentViewModel,
  BetrokkeneMetKlantContact as BetrokkeneWithKlantContact,
  ExpandedKlantContactApiViewmodel,
  ContactverzoekViewmodel,
  InternetaakApiViewModel,
  ActorApiViewModel,
} from "./types";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesInterneTaken = `${klantinteractiesBaseUrl}/internetaken`;
const klantinteractiesActoren = `${klantinteractiesBaseUrl}/actoren`;
const klantinteractiesPartijen = `${klantinteractiesBaseUrl}/partijen`;
const klantinteractiesDigitaleadressen = `${klantinteractiesBaseUrl}/digitaleadressen`;

////////////////////////////////////////////
// contactmomenten
export function mapToContactmomentViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
) {
  const viewmodel = value.page.map((x) => {
    const medewerker = x.klantContact?.hadBetrokkenActoren?.find(
      (x: { soortActor: string }) => x.soortActor === "medewerker",
    );
    return {
      url: x.klantContact.url,
      registratiedatum: x.klantContact?.plaatsgevondenOp,
      kanaal: x.klantContact?.kanaal,
      tekst: x.klantContact?.inhoud,
      objectcontactmomenten: [], //wordt uitgesteld. besproken in https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/800
      medewerkerIdentificatie: {
        identificatie: medewerker?.actorIdentificator?.objectId || "",
        voorletters: "",
        achternaam: medewerker?.naam || "",
        voorvoegselAchternaam: "",
      },
    };
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactmomentViewModel> = {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: viewmodel,
  };

  return paginatedContactenviewmodel;
}

////////////////////////////////////////////
// contactmomenten and contactverzoeken
export async function enrichBetrokkeneWithKlantContact(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkene of value.page) {
    const searchParams = new URLSearchParams();
    searchParams.set("hadBetrokkene__uuid", betrokkene.uuid);
    const url = `${klantinteractiesKlantcontacten}?${searchParams.toString()}`;
    await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          betrokkene.klantContact = d
            .page[0] as ExpandedKlantContactApiViewmodel; // er is altijd maar 1 contact bij een betrokkeke!
        }
      });
  }
  return value;
}

////////////////////////////////////////////
// contactverzoeken
export async function enrichKlantcontactWithInterneTaak(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    const searchParams = new URLSearchParams();
    searchParams.set(
      "klantcontact__uuid",
      betrokkeneWithKlantcontact.klantContact.uuid,
    );
    const url = `${klantinteractiesInterneTaken}?${searchParams.toString()}`;
    await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          betrokkeneWithKlantcontact.klantContact.internetaak = d
            .page[0] as InternetaakApiViewModel; //we mogen er vanuit gaan dat er 1 'hoofd interen tak' is bj een contact moment.
          // het model ondersteunt meerdere vervolg contacten, maar daar houden we binnen kiss nog geen rekening mee.
        }
      });
  }

  return value;
}

export function filterOutContactmomenten(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<BetrokkeneWithKlantContact> {
  const filtered = value.page.filter((item) => item?.klantContact?.internetaak);
  return {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: filtered,
  };
}

export function mapToContactverzoekViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<ContactverzoekViewmodel> {
  const viewmodel = value.page.map((x) => {
    console.log("gggggggggggggggg", x);

    return {
      url: x.klantContact.internetaak.url,
      medewerker:
        x.klantContact.hadBetrokkenActoren &&
        x.klantContact.hadBetrokkenActoren.length > 0
          ? x.klantContact.hadBetrokkenActoren[0].naam
          : "",
      onderwerp: x.klantContact.onderwerp,
      record: {
        startAt: x.klantContact.internetaak.toegewezenOp,
        data: {
          status: x.klantContact.internetaak.status,
          contactmoment: x.klantContact.url,
          registratiedatum: x.klantContact.plaatsgevondenOp,
          datumVerwerkt: x.klantContact.internetaak.afgehandeldOp,
          toelichting: x.klantContact.internetaak.toelichting,
          actor: {
            naam: x.klantContact.internetaak?.actor?.naam,
            soortActor: x.klantContact.internetaak?.actor?.soortActor,
            identificatie: "",
            //todo overige soort specifieke actor velden
            //naamOrganisatorischeEenheid: "", //todo: ???
            //typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling, //todo: ???
            //identificatieOrganisatorischeEenheid: "", //todo: ???
            //kan nu nog niet want we kunnen deze gegevens nog niet invoeren in het nieuwe openklant
          },

          betrokkene: {
            rol: "klant",
            klant: x.partij.klant,
            persoonsnaam: x.partij.persoonsnaam,
            organisatie: x.partij.organisatie,
            digitaleAdressen: x.digitaleAdressenExpanded,
          },

          verantwoordelijkeAfdeling: "", //todo: waar komt dit vandaan?

          //mogen we ervan uitgaan dat de medewerker de enige betrokken actor is?
        },
      },
    } as ContactverzoekViewmodel;
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactverzoekViewmodel> =
    {
      next: value.next,
      previous: value.previous,
      count: value.count,
      page: viewmodel,
    };

  return paginatedContactenviewmodel;
}

export async function enrichInterneTakenWithActoren(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    const actoren =
      betrokkeneWithKlantcontact?.klantContact?.internetaak
        ?.toegewezenAanActoren;
    for (const actor of actoren) {
      //let op. dit is eigenelijk niet volgens klantinteractie api specs.
      //toegewezen actoren zou alleen een lijst id's bevatten
      //klopppen de specs niet of de implementatie?
      const url = `${klantinteractiesActoren}/${actor.uuid}`;
      await fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((d) => {
          betrokkeneWithKlantcontact.klantContact.internetaak.actor =
            d as ActorApiViewModel;
        });
    }
  }

  return value;
}

export async function enrichInterneTakenWithBetrokkene(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    //in principe hoeft dit maar 1 keer. alle contactverzoeken zouden dezelfde betrokkeke gegevens moeten hebben.
    //voor zekerheid wel allemaal apart ophalen. eventueel latere optimaliseren als data accuraat genoeg blijkt te zijn

    const partijId = betrokkeneWithKlantcontact?.wasPartij.uuid;
    if (partijId) {
      const url = `${klantinteractiesPartijen}/${partijId}?`;

      await fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then(async (d) => {
          if (d.partijIdentificatie) {
            betrokkeneWithKlantcontact.partij = {
              klant: url,
              persoonsnaam: {
                voornaam: d?.partijIdentificatie?.contactnaam?.voornaam,
                voorvoegselAchternaam:
                  d?.partijIdentificatie?.contactnaam?.voorvoegselAchternaam,
                achternaam: d?.partijIdentificatie?.contactnaam?.achternaam,
              },
              organisatie: "", //verplicht maar leeg voor een persoon?
              rol: "klant",
            };
          }
        });
    }
  }

  return value;
}

export async function enrichBetrokkeneWithDigitaleAdressen(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    betrokkeneWithKlantcontact.digitaleAdressenExpanded = [];
    const digitaleAdressen = betrokkeneWithKlantcontact?.digitaleAdressen;
    for (const digitaalAdres of digitaleAdressen) {
      const url = `${klantinteractiesDigitaleadressen}/${digitaalAdres.uuid}?`;
      await fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then(async (d) => {
          betrokkeneWithKlantcontact.digitaleAdressenExpanded.push({
            adres: d.adres,
            soortDigitaalAdres: d.soortDigitaalAdres,
            omschrijving: d.omschrijving,
          });
        });
    }
  }

  return value;
}

export function fetchBetrokkene(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as BetrokkeneWithKlantContact));
}
