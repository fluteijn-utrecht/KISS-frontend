import type {
  ContactmomentKlant,
  ContactmomentState,
} from "@/stores/contactmoment";
import type { Klant } from "../klant/types";

export function getKlantInfo(contactmoment: ContactmomentState) {
  const klanten = contactmoment.vragen
    .flatMap(({ klanten }) => klanten)
    .filter(({ shouldStore }) => shouldStore)
    .map(({ klant }) => klant);

  const infos = klanten.map(_getKlantInfo);

  return infos.find((info) => info.name || info.contact);
}

function _getKlantInfo(klant: ContactmomentKlant) {
  const name =
    [klant.voornaam, klant.voorvoegselAchternaam, klant.achternaam]
      .filter(Boolean)
      .join(" ") || klant.bedrijfsnaam;

  const email = klant.emails.map(({ email }) => email).find(Boolean);
  const phone = klant.telefoonnummers
    .map(({ telefoonnummer }) => telefoonnummer)
    .find(Boolean);

  const contact = email || phone;

  return {
    name,
    contact,
  };
}
