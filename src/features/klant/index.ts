export * from "./types";

export { default as PersoonZoeker } from "./persoon/PersoonZoeker.vue";
export { default as BrpGegevens } from "./persoon/BrpGegevens.vue";

export { default as BedrijfZoeker } from "./bedrijf/BedrijfZoeker.vue";
export { default as HandelsregisterGegevens } from "./bedrijf/HandelsregisterGegevens.vue";

export { default as KlantDetails } from "./KlantDetails.vue";
export { default as ContactZoeker } from "./contact/ContactZoeker.vue";

export { usePersoonByBsn } from "./persoon/service";
export { useBedrijfByVestigingsnummer } from "./bedrijf/service";
export { useKlantById } from "./service";
