<template><slot :enriched="result"></slot></template>
<script setup lang="ts">
import { mapServiceData, ServiceResult } from "@/services";
import { computed, reactive } from "vue";
import { useRouter } from "vue-router";
import type { Bedrijf as Bedrijf, EnrichedBedrijf } from "../types";
import { useEnrichedBedrijf } from "./bedrijf-enricher";
import { useOrganisatieIds } from "@/stores/user";
import type { Klant } from "../../types";
import { ensureKlantForVestigingsnummer } from "../../service";

const props = defineProps<{ record: Bedrijf | Klant }>();
const [bedrijfIdentificatie, klantData, handelsregisterData] =
  useEnrichedBedrijf(() => props.record);

const email = mapServiceData(klantData, (k) => k?.emailadres || "");
const telefoonnummer = mapServiceData(
  klantData,
  (k) => k?.telefoonnummer || "",
);

const getKlantUrl = (klant: Klant) => `/bedrijven/${klant.id}`;

function mapLink(klant: Klant | null, naam: string | null) {
  return (
    klant && {
      to: getKlantUrl(klant),
      title: `Details ${naam}`,
    }
  );
}

const router = useRouter();
const organisatieIds = useOrganisatieIds();

const klantBedrijfsnaam = mapServiceData(
  klantData,
  (k) => k?.bedrijfsnaam ?? "",
);

const handelsBedrijfsnaam = mapServiceData(handelsregisterData, (k) => {
  console.log(k);
  return k?.bedrijfsnaam ?? "";
});

const bedrijfsnaam = computed(() => {
  if (handelsBedrijfsnaam.success && handelsBedrijfsnaam.data)
    return handelsBedrijfsnaam;
  if (klantBedrijfsnaam.success && klantBedrijfsnaam.data)
    return klantBedrijfsnaam;
  if (klantBedrijfsnaam.loading || klantBedrijfsnaam.loading)
    return ServiceResult.loading();
  return klantBedrijfsnaam;
});

const detailLink = computed(() => {
  const n = bedrijfsnaam.value.success ? bedrijfsnaam.value?.data : null;
  return mapServiceData(klantData, (k) => mapLink(k, n ?? null));
});

const create = async () => {
  //We maken bij een kvk bedrijf een klant aan.
  //hier moeten we weten of de klant met een vestigingsnr of een ander id aangemaakt moet worden
  //voor niet natuurlijke personen gebruiken we rsin
  //voor overige bedrijven het vestigingsnummer
  if (!bedrijfIdentificatie.value) throw new Error();

  const bedrijfsnaam = handelsBedrijfsnaam.success
    ? handelsBedrijfsnaam.data
    : "";

  let identifier;

  if (handelsregisterData.success) {
    if (handelsregisterData.data?.vestigingsnummer) {
      identifier = {
        vestigingsnummer: handelsregisterData.data.vestigingsnummer,
      };
    } else if (handelsregisterData.data?.rsin) {
      identifier = { rsin: handelsregisterData.data.rsin };
    }
  }

  if (!identifier) {
    return;
  }

  const newKlant = await ensureKlantForVestigingsnummer(
    {
      identifier: identifier,
      bedrijfsnaam,
    },
    organisatieIds.value[0] || "",
  );

  const url = getKlantUrl(newKlant);
  router.push(url);
};

const result: EnrichedBedrijf = reactive({
  bedrijfsnaam,
  kvkNummer: mapServiceData(handelsregisterData, (h) => h?.kvkNummer ?? ""),
  postcodeHuisnummer: mapServiceData(handelsregisterData, (h) =>
    [h?.postcode, h?.huisnummer].join(" "),
  ),
  email,
  telefoonnummer,
  detailLink,
  create,
});
</script>
