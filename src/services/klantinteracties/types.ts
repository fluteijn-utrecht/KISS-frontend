import type { Contactverzoek } from "@/features/contact/contactverzoek/overzicht/types";

export type DigitaalAdres = {
  adres: string;
  soortDigitaalAdres?: string;
  omschrijving?: string;
};

export type BetrokkeneMetKlantContact = {
  uuid: string;
  wasPartij: { uuid: string; url: string };
  klantContact: ExpandedKlantContactApiViewmodel;
  partij: {
    rol: "klant";
    klant: string;
    persoonsnaam: {
      voornaam: string;
      voorvoegselAchternaam: string;
      achternaam: string;
    };
    organisatie: string;
    //
  };
  digitaleAdressen: Array<{ uuid: string; url: string }>;
  digitaleAdressenExpanded: Array<DigitaalAdres>;
};

export type ExpandedKlantContactApiViewmodel = {
  uuid: string;
  url: string;
  plaatsgevondenOp: string;
  kanaal: string;
  inhoud: string;
  onderwerp: string;
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
  toegewezenAanActoren: Array<{
    uuid: string;
    url: string;
  }>;
  toelichting: string;
  status: string;
  toegewezenOp: string;
  afgehandeldOp: string;
  actor: ActorApiViewModel;
};

export type ActorApiViewModel = {
  uuid: string;
  url: string;
  naam: string;
  soortActor: string;
  indicatieActief: boolean;
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
