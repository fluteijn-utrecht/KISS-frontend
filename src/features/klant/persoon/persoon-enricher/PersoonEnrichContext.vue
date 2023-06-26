<template><slot :enriched="result"></slot></template>
<script setup lang="ts">
import { mapServiceData, ServiceResult, type ServiceData } from "@/services";
import { computed, reactive } from "vue";
import { ensureKlantForBsn } from "../../service";
import type { EnrichedPersoon, Persoon } from "../types";
import { useRouter } from "vue-router";
import { useEnrichedPersoon } from "./persoon-enricher";
import type { Klant } from "../../types";
import { useOrganisatieIds } from "@/stores/user";

const props = defineProps<{ record: Klant | Persoon }>();

const [bsn, klantData, persoonData] = useEnrichedPersoon(() => props.record);

const getKlantUrl = (klant: Klant) => `/personen/${klant.id}`;

function mapNaam(klantOrPersoon: Klant | Persoon | null) {
  return (
    klantOrPersoon &&
    [
      klantOrPersoon.voornaam,
      klantOrPersoon.voorvoegselAchternaam,
      klantOrPersoon.achternaam,
    ]
      .filter(Boolean)
      .join(" ")
  );
}

function mapTelefoonnummers(klant: Klant | null) {
  return klant?.telefoonnummer ?? null;
}

function mapEmails(klant: Klant | null) {
  return klant?.emailadres ?? null;
}

function mapLink(klant: Klant | null, naam: string | null) {
  return (
    klant && {
      to: getKlantUrl(klant),
      title: `Details ${naam}`,
    }
  );
}

function mapGeboortedatum(persoon: Persoon | null) {
  return persoon?.geboortedatum ?? null;
}

const router = useRouter();
const organisatieIds = useOrganisatieIds();

const create = async () => {
  if (!bsn.value) throw new Error();
  const naam = persoonData.success
    ? {
        achternaam: persoonData.data?.achternaam,
        voornaam: persoonData.data?.voornaam,
        voorvoegselAchternaam: persoonData.data?.voorvoegselAchternaam,
      }
    : {};
  const newKlant = await ensureKlantForBsn(
    {
      bsn: bsn.value,
      ...naam,
    },
    organisatieIds.value[0] || ""
  );
  const url = getKlantUrl(newKlant);
  router.push(url);
};

const klantNaam = mapServiceData(klantData, mapNaam);
const persoonNaam = mapServiceData(persoonData, mapNaam);

const naam = computed<ServiceData<string | null>>(() => {
  if (klantNaam.loading && persoonNaam.loading) return klantNaam;
  if (klantNaam.success && klantNaam.data) return klantNaam;
  if (persoonNaam.success && persoonNaam.data) return persoonNaam;
  return ServiceResult.success(null);
});

const telefoonnummers = mapServiceData(klantData, mapTelefoonnummers);

const emails = mapServiceData(klantData, mapEmails);

const geboortedatum = mapServiceData(persoonData, mapGeboortedatum);

const adresregel1 = mapServiceData(persoonData, (k) => k?.adresregel1 || null);
const adresregel2 = mapServiceData(persoonData, (k) => k?.adresregel2 || null);
const adresregel3 = mapServiceData(persoonData, (k) => k?.adresregel3 || null);

const detailLink = computed(() => {
  const n = naam.value.success ? naam.value.data : null;
  return mapServiceData(klantData, (k) => mapLink(k, n));
});

const result: EnrichedPersoon = reactive({
  naam,
  bsn,
  telefoonnummers,
  emails,
  geboortedatum,
  adresregel1,
  adresregel2,
  adresregel3,
  create,
  detailLink,
});
</script>
