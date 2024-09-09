import type { Contactverzoek } from "@/features/contact/contactverzoek/overzicht/types";

export type BetrokkeneMetKlantContact = {
  uuid: string;
  klantContact: ExpandedKlantContactApiViewmodel;
};

export type ExpandedKlantContactApiViewmodel = {
  uuid: string;
  url: string;
  plaatsgevondenOp: string;
  kanaal: string;
  inhoud: string;
  hadBetrokkenActoren: Array<{
    soortActor: string;
    naam: string;
    actorIdentificator: {
      objectId: string;
    };
  }>;
  internetaak: InternetaakApiViewModel;
};

export type InternetaakApiViewModel = {
  uuid: string;
  url: string;
  nummer: string;
  gevraagdeHandeling: string;
  toegewezenAanActor: {
    uuid: string;
    url: string;
  };
  toelichting: string;
  status: string;
  toegewezenOp: string;
  afgehandeldOp: string;
};

export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
}

export interface ContactmomentViewModel {
  url: string;
  registratiedatum: string;
  kanaal: string;
  tekst: string;
  objectcontactmomenten: {
    object: string;
    objectType: "zaak";
    contactmoment: string;
  }[];
  medewerkerIdentificatie: MedewerkerIdentificatie;
}

///////////////////////////////

//todo: Contactverzoek type verplaatsen naar hier. o meer specifieke models introduceren
export type ContactverzoekViewmodel = Contactverzoek;
