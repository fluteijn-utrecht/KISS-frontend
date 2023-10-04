export type ObjectWrapper<T> = {
  url: string;
  uuid: string;
  type: string;
  record: {
    index: number;
    typeVersion: number;
    data: T;
    startAt?: string;
    endAt?: string;
    registrationAt?: string;
    correctionFor: unknown;
    correctedBy: unknown;
  };
};
export type KlantTaak = {
  data?: {
    [k: string]: unknown;
  };
  zaak?: string;
  title?: string;
  status: "open" | "ingediend" | "verwerkt" | "gesloten";
  formulier: {
    type?: "id" | "url";
    value?: string;
  };
  verloopdatum?: string;
  identificatie: {
    type?: string;
    value?: string;
  };
  verzonden_data?: {
    [k: string]: unknown;
  };
  verwerker_taak_id: string;
};
