export interface ContactmomentViewModel {
  url: string;
  registratiedatum: string;
  medewerker: string;
  kanaal: string;
  tekst: string;
  objectcontactmomenten: {
    object: string;
    objectType: "zaak";
    contactmoment: string;
  }[];
  medewerkerIdentificatie: MedewerkerIdentificatie;
}

export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
}

export interface ContactmomentZaak {
  status: string;
  zaaktype: string;
  zaaknummer: string;
}

export interface ContactmomentContactverzoek {
  medewerker: string;
  completed?: Date;
}
