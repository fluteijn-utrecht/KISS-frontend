import type { ContactverzoekForm } from "@/features/contactverzoek";
import type { Klant } from "@/features/klant/types";
import type { Zaak } from "@/features/zaaksysteem/types";
import { getFormattedUtcDate } from "@/services";
import { defineStore } from "pinia";
import { resetAllState } from "../create-store";
export * from "./types";

export type ContactmomentZaak = { zaak: Zaak; shouldStore: boolean };

export interface Vraag {
  zaken: ContactmomentZaak[];
  notitie: string;
  contactverzoek: ContactverzoekForm;
  startdatum: string;
  kanaal: string;
  resultaat: string;
  klanten: { klant: Klant; shouldStore: boolean }[];
}

function initVraag(): Vraag {
  return {
    zaken: [],
    notitie: "",
    contactverzoek: {
      voornaam: "",
      voorvoegselAchternaam: undefined,
      achternaam: "",
      telefoonnummer1: "",
      telefoonnummer2: "",
      emailadres: "",
      attendee: "",
      description: "",
      useKlantFromStore: false,
      isDirty: false,
      isSubmitted: false,
      url: undefined,
    },
    startdatum: getFormattedUtcDate(),
    kanaal: "",
    resultaat: "",
    klanten: [],
  };
}

interface ContactmomentState {
  contactmomentLoopt: boolean;
  vragen: Vraag[];
  huidigeVraag: Vraag;
}

export const useContactmomentStore = defineStore("contactmoment", {
  state: () => {
    const huidigeVraag = initVraag();
    return {
      contactmomentLoopt: false,
      vragen: [huidigeVraag],
      huidigeVraag,
    } as ContactmomentState;
  },
  getters: {
    klant(state): Klant | undefined {
      return state.huidigeVraag?.klanten
        ?.filter((x) => x.shouldStore)
        ?.map((x) => x.klant)?.[0];
    },
    huidigeVraagIndex: (state) => state.vragen.indexOf(state.huidigeVraag),
  },
  actions: {
    start() {
      if (this.contactmomentLoopt) return;
      this.contactmomentLoopt = true;
    },
    startNieuweVraag() {
      if (!this.contactmomentLoopt) return;
      const nieuweVraag = initVraag();
      if (this.huidigeVraag?.klanten) {
        nieuweVraag.klanten = this.huidigeVraag.klanten.map(
          (klantKoppeling) => ({
            ...klantKoppeling,
          })
        );
      }
      this.vragen.push(nieuweVraag);
      this.huidigeVraag = nieuweVraag;
    },
    stop() {
      this.$reset();
      // Temporary. When we implement multiple running contactmomenten, each will have it's own state
      resetAllState();
    },
    addZaak(zaak: Zaak) {
      if (!this.huidigeVraag) return;

      const match = this.huidigeVraag.zaken.find(
        (element) => element.zaak.id === zaak.id
      );

      if (match) {
        match.zaak = zaak;
        match.shouldStore = true;
        return;
      }

      //als de zaak nog niet gekoppeld was aan het contact moment dan voegen we hem eerst toe
      this.huidigeVraag.zaken.push({
        zaak,
        shouldStore: true,
      });
    },
    toggleZaak(zaak: Zaak) {
      if (!this.huidigeVraag) return false;

      const match = this.huidigeVraag.zaken.find(
        (element) => element.zaak.id === zaak.id
      );

      if (match) {
        match.zaak = zaak;
        match.shouldStore = !match.shouldStore;
        return;
      }

      this.huidigeVraag.zaken.push({
        zaak,
        shouldStore: true,
      });
    },
    isZaakLinkedToContactmoment(id: string) {
      return (
        this.huidigeVraag?.zaken?.some(
          ({ zaak, shouldStore }) => shouldStore && zaak.id === id
        ) ?? false
      );
    },
    setKlant(klant: Klant) {
      if (!this.huidigeVraag) return;
      const match = this.huidigeVraag.klanten.find(
        (x) => x.klant.id === klant.id
      );

      this.huidigeVraag.klanten.forEach((x) => {
        x.shouldStore = false;
      });

      if (match) {
        match.klant = klant;
        match.shouldStore = true;
        return;
      }

      this.huidigeVraag.klanten.push({
        shouldStore: true,
        klant,
      });
    },
    setNotitie(notitie: string) {
      if (!this.huidigeVraag) return;
      this.huidigeVraag.notitie = notitie;
    },
  },
});
