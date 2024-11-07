export type ContactverzoekOverzichtItem = {
  url: string;
  registratiedatum: string;
  onderwerp: string;
  status: string;
  aangemaaktDoor: string;
  behandelaar: string;
  toelichtingVoorCollega: string;
  toelichtingBijContactmoment: string;
  betrokkene?: {
    isGeauthenticeerd: boolean;
    persoonsnaam: {
      voorletters?: string;
      voornaam?: string;
      voorvoegselAchternaam?: string;
      achternaam?: string;
    };
    organisatie?: string;
    digitaleAdressen: {
      omschrijving?: string;
      soortDigitaalAdres?: string;
      adres: string;
    }[];
  };
  vraag: string;
  objecten: {
    object: string;
    objectType: string;
    contactmoment: string;
  }[];
};
