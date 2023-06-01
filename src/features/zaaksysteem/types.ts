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

export type InformatieObject = {
  //uuid: string;
  informatieobjectId: string;
  // titel: string;
  // beschrijving: string;
};
