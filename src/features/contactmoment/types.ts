export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
}

export interface Contactmoment {
  bronorganisatie: string; //verplicht in de api
  registratiedatum: string; //2019-08-24T14:15:22Z //serverside?
  kanaal: string;
  tekst: string;
  onderwerpLinks: string[];
  initiatiefnemer: string;
  medewerkerIdentificatie: MedewerkerIdentificatie | undefined; //serverside?
  //bovenstaande slaan we op bij een contactmoment.
  //de rest is mogelijk obsolete.
  //wellicht nog te gebruiken voor oa contactverzoeken
  vorigContactmoment: string | undefined;
  voorkeurskanaal: string;
  voorkeurstaal: string;
  medewerker: string;
  resultaat: string;
  startdatum: string;
  einddatum: string;
  gespreksId?: string;
  primaireVraag?: string;
  primaireVraagWeergave?: string;
  afwijkendOnderwerp?: string;
}

export interface Gespreksresultaat {
  id: string;
  definitie: string;
}

export interface ContactmomentObject {
  contactmoment: string;
  object: string;
  objectType: string;
}

export interface ContactverzoekDetail {
  id: string;
  datum: string;
  status: string;
  behandelaar: string;
  afgerond: string;
  starttijd: string;
  aanmaker: string;
  notitie: string;
  primaireVraagWeergave?: string;
  afwijkendOnderwerp?: string;
}
