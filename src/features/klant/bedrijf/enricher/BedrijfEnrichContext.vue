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
  console.log("--klant", klant, naam);
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
    console.log(" handelsBedrijfsnaam ", handelsBedrijfsnaam.data);
  if (klantBedrijfsnaam.success && klantBedrijfsnaam.data)
    console.log(" klantBedrijfsnaam ", klantBedrijfsnaam.data);

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
  console.log(
    "ok we have got a bedrijfsnaam, now make a detail link",
    bedrijfsnaam.value.success,
    bedrijfsnaam.value?.data,
    klantData.success,
  );
  return mapServiceData(klantData, (k) => mapLink(k, n ?? null));
});

const create = async () => {
  console.log("-------0");
  // console.log("-------");
  // if (klantData.success && handelsregisterData.success) {
  //   console.log(

  //     handelsregisterData.data,
  //   );
  // }
  // console.log("-------");

  //hier moeten we weten of de klant met een vestigingsnr of een ander id aangemaakt moet worden
  //dus voor bedrijfIdentificatie moet door de enricher ook een ander ding teruggeveen kunnen worden
  if (!bedrijfIdentificatie.value) throw new Error();

  const bedrijfsnaam = handelsBedrijfsnaam.success
    ? handelsBedrijfsnaam.data
    : "";

  let x;

  if (handelsregisterData.success) {
    if (handelsregisterData.data?.vestigingsnummer) {
      x = { vestigingsnummer: handelsregisterData.data.vestigingsnummer };
    } else if (handelsregisterData.data?.rsin) {
      x = { rsin: handelsregisterData.data.rsin };
    }
  }

  if (!x) {
    return;
  }

  const newKlant = await ensureKlantForVestigingsnummer(
    {
      identifier: x,
      bedrijfsnaam,
    },
    organisatieIds.value[0] || "",
  );

  // let newKlant = null;

  // if (vestigingsnummer.value) {
  //   newKlant = await ensureKlantForVestigingsnummer(
  //     {
  //       vestigingsnummer: vestigingsnummer.value,
  //       bedrijfsnaam,
  //     },
  //     organisatieIds.value[0] || "",
  //   );
  // } else {
  //   newKlant = await ensureKlantForNietNatuurlijkPersoon(
  //     {
  //       bedrijfsnaam,
  //       id: handelsregisterData..state..value,
  //     },
  //     organisatieIds.value[0] || "",
  //   );
  // }

  console.log("-------1");
  const url = getKlantUrl(newKlant);
  console.log("-------2");
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

  // subjectType: mapServiceData(handelsregisterData, (h) => h?..subjectType ?? ""),
  // subjectIdentificatie: mapServiceData(
  //   handelsregisterData,
  //   (h) => h?.subjectIdentificatie ?? "",
  // ),
});
</script>
