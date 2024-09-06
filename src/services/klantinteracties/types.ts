import type { Contactverzoek } from "@/features/contact/contactverzoek/overzicht/types";

export type BetrokkeneMetKlantContact = {
  uuid: string;
  klantContact: ExpandedKlantContactViewmodel;
};

export type ExpandedKlantContactViewmodel = {
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
