export type ContactverzoekData = {
  status: string;
  contactmoment: string;
  registratiedatum: string;
  datumVerwerkt?: string;
  toelichting?: string;
  actor: {
    naam: string;
    soortActor: string;
    identificatie: string;
    naamOrganisatorischeEenheid?: string;
    typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid;
    identificatieOrganisatorischeEenheid?: string;
  };
  betrokkene: {
    rol: "klant";
    klant?: string;
    persoonsnaam?: {
      voornaam?: string;
      voorvoegselAchternaam?: string;
      achternaam?: string;
    };
    organisatie?: string;
    digitaleAdressen: {
      adres: string;
      soortDigitaalAdres?: string;
      omschrijving?: string;
    }[];
  };
  verantwoodelijkeAfdeling: string;
};

export type NewContactverzoek = {
  record: {
    typeVersion: number;
    startAt: string;
    data: ContactverzoekData;
  };
};

export type Contactverzoek = NewContactverzoek & {
  url: string;
};

export enum TypeOrganisatorischeEenheid {
  Groep = "groep",
  Afdeling = "afdeling",
}


