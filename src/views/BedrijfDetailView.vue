<template>
  <back-link />
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>

  <tab-list v-model="currentTab">
    <tab-list-item label="Contactgegevens">
      <template #default="{ setError, setLoading }">
        <klant-details
          v-if="defaultSysteem"
          :klant-id="bedrijfId"
          :systeem="defaultSysteem"
          @load="klant = $event"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-data-item
      label="KvK-gegevens"
      :data="bedrijf"
      :disabled="(b) => !b"
    >
      <template #success="{ data }">
        <handelsregister-gegevens v-if="data" :bedrijf="data" />
      </template>
    </tab-list-data-item>
    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactmomenten-for-klant-identificator
          v-if="systemen && bedrijf.success && bedrijf.data"
          :klant-identificator="bedrijf.data"
          :systemen="systemen"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview :zaakurl="object.object" />
          </template>
        </contactmomenten-for-klant-identificator>
      </template>
    </tab-list-item>
    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(z) => !z.count">
      <template #success="{ data }">
        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>
    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactverzoeken-for-klant-identificator
          v-if="systemen && bedrijf.success && bedrijf.data"
          :klant-identificator="bedrijf.data"
          :systemen="systemen"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview v-if="object.object" :zaakurl="object.object" />
          </template>
        </contactverzoeken-for-klant-identificator>
      </template>
    </tab-list-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { KlantDetails } from "@/features/klant/klant-details";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import {
  useZakenByKlantBedrijfIdentifier,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem, TabListItem } from "@/components/tabs";

import BackLink from "@/components/BackLink.vue";
import { HandelsregisterGegevens } from "@/features/bedrijf/bedrijf-details";
import { useBedrijfByIdentifier } from "@/features/bedrijf/use-bedrijf-by-identifier";
import type { BedrijfIdentifier } from "@/services/kvk";
import ContactverzoekenForKlantIdentificator from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantIdentificator.vue";
import ContactmomentenForKlantIdentificator from "@/features/contact/contactmoment/ContactmomentenForKlantIdentificator.vue";
import { useLoader } from "@/services";
import { fetchSystemen } from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";

defineProps<{ bedrijfId: string }>();

const contactmomentStore = useContactmomentStore();

const currentTab = ref("");
const klant = ref<Klant>();

const getBedrijfIdentifier = (): BedrijfIdentifier | undefined => {
  if (!klant.value) return undefined;
  if (klant.value.vestigingsnummer)
    return {
      vestigingsnummer: klant.value.vestigingsnummer,
    };
  // if (klant.data.rsin)
  //   return {
  //     rsin: klant.data.rsin,
  //     kvkNummer: klant.data.kvkNummer,
  //   };
  if (klant.value.nietNatuurlijkPersoonIdentifier)
    return {
      //gechoogel met params verschil ok1 en esuite
      rsin: klant.value.nietNatuurlijkPersoonIdentifier,
    };
  if (klant.value.rsin)
    return {
      //gechoogel met params verschil ok1 en esuite
      rsin: klant.value.rsin,
    };
};

const bedrijf = useBedrijfByIdentifier(getBedrijfIdentifier);

const zaken = useZakenByKlantBedrijfIdentifier(() => {
  if (!bedrijf.success || !bedrijf.data?.kvkNummer) return undefined;
  if (bedrijf.data.vestigingsnummer)
    return { vestigingsnummer: bedrijf.data.vestigingsnummer };
  if (bedrijf.data.rsin)
    return { rsin: bedrijf.data.rsin, kvkNummer: bedrijf.data.kvkNummer };
});

watch(
  () =>
    klant.value && bedrijf.success
      ? ([klant.value, bedrijf.data] as const)
      : [],
  ([k, b]) => {
    if (!k || !b) return;
    contactmomentStore.setKlant({
      ...k,
      ...b,
      hasContactInformation:
        (k.emailadressen && k.emailadressen.length > 0) ||
        (k.telefoonnummers && k.telefoonnummers.length > 0),
    });
  },
  { immediate: true },
);

const { data: systemen } = useLoader(() => fetchSystemen());
const defaultSysteem = computed(() => systemen.value?.find((x) => x.isDefault));
</script>
