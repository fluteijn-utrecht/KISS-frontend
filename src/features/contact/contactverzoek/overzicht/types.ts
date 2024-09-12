import type { NewContactverzoek } from "../../components/types";

export type Contactverzoek = NewContactverzoek & {
  url: string;
  medewerker: string;
  onderwerp: string;
};
