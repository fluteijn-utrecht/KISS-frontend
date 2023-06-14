export interface ContactmomentViewModel {
  id: string;
  url: string;
  startdatum: Date;
  registratiedatum: Date;
  medewerker: string;
  kanaal: string;
  resultaat: string;
  tekst: string;
  zaken: string[];
  contactverzoeken: ContactmomentContactverzoek[];
  _self: {
    owner: string;
  };
  primaireVraag?: string;
  primaireVraagWeergave?: string;
  afwijkendOnderwerp?: string;
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

export interface KlantContactmoment {
  url: string;
  contactmoment: string;
  klant: string;
  rol: string;
}

export interface ObjectContactmoment {
  url: string;
  contactmoment: string;
  object: string;
  objectType: string;
}
