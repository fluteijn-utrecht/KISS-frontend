import type { NewContactverzoek } from "../../components/types";

export type Contactverzoek = NewContactverzoek & {
  url: string;
  medewerker: string | undefined; // ALLEEN IN OK2???
  onderwerp: string | undefined; // ALLEEN IN OK2???
  toelichting: string | undefined; // ALLEEN IN OK2???
};
