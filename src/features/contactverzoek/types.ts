export type NewContactverzoek = {
  record: {
    typeVersion: number;
    startAt: string;
    data: {
      status: string;
      contactmoment: string;
      registratiedatum: string;
      datumVerwerkt?: string;
      toelichting?: string;
      actor: {
        identificatie: string;
        soortActor: string;
        naam: string;
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
    };
  };
};

export type Contactverzoek = NewContactverzoek & {
  url: string;
};
