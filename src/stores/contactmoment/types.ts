import type { Klant } from "@/features/klant/types";

export type NieuweKlant = Pick<
  Klant,
  | "voornaam"
  | "voorvoegselAchternaam"
  | "achternaam"
  | "telefoonnummer"
  | "emailadres"
>;

export interface Medewerker {
  id: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  emailadres: string;
  telefoonnummer1?: string;
  telefoonnummer2?: string;
}
