export type ZaakDetails = {
  id: string;
  identificatie: string;
  toelichting: string;
  startdatum?: Date;
  bronorganisatie: string;
  verantwoordelijkeOrganisatie: string;
  zaaktype: string;
  zaaktypeLabel: string;
  zaaktypeOmschrijving: string;
  status: string;
  behandelaar: string;
  aanvrager: string;
  fataleDatum?: Date;
  streefDatum?: Date;
  indienDatum?: Date;
  registratieDatum?: Date;
  self: string;
  documenten?: ZaakDocument[];
  omschrijving: string;
};

export interface ZaakDocument {
  id: string;
  titel: string;
  bestandsomvang: number;
  creatiedatum: Date;
  vertrouwelijkheidaanduiding: string;
  formaat: string;
  downloadUrl: string;
}

export type ZaakType = {
  id: string;
  onderwerp: string;
  omschrijving: string;
  doorlooptijd: string;
  servicenorm: string;
};

export type OrganisatorischeEenheidType = {
  naam: string;
};

export type VestigingType = {
  naam: string;
  vestigingsNummer: string;
};

export type NietNatuurlijkPersoonType = {
  statutaireNaam: string;
};

export type NatuurlijkPersoonType = {
  voornamen: string;
  voorvoegselGeslachtsnaam: string;
  geslachtsnaam: string;
};

export type MedewerkerType = {
  achternaam: string;
  geslachtsnaam: string;
  identificatie: string;
  voorletters: string;
  voorvoegselAchternaam: string;
};

export type RolType = {
  url: string;
  uuid: string;
  zaak: string;
  betrokkene: string;
  betrokkeneType: string;
  roltype: string;
  omschrijving: string;
  omschrijvingGeneriek: string;
  roltoelichting: string;
  registratiedatum: string;
  indicatieMachtiging: string;
  betrokkeneIdentificatie:
    | MedewerkerType
    | OrganisatorischeEenheidType
    | VestigingType
    | NietNatuurlijkPersoonType
    | NatuurlijkPersoonType;
};

// export type StatusType = {
//   url: string;
//   uuid: string;
//   zaak: string;
//   statustype: string;
//   datumStatusGezet: string;
//   statustoelichting: string;
// };

// export type StatusTypeType = {
//   url: string;
//   omschrijving: string;
//   omschrijvingGeneriek: string;
//   statustekst: string;
//   zaaktype: string;
//   volgnummer: string;
//   isEindstatus: string;
//   informeren: string;
// };
