<template>
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>
  <nav>
    <ul>
      <li>
        <router-link :to="{ name: 'bedrijven' }">{{
          "< Bedrijven zoeken"
        }}</router-link>
      </li>
    </ul>
  </nav>
  <tab-list v-model="currentTab">
    <tab-list-data-item
      label="Contactgegevens"
      :data="klant"
      :disabled="(k) => !k"
    >
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>
    <tab-list-data-item
      label="KvK-gegevens"
      :data="bedrijf"
      :disabled="(b) => !b"
    >
      <template #success="{ data }">
        <handelsregister-gegevens v-if="data" :bedrijf="data" />
      </template>
    </tab-list-data-item>
    <tab-list-data-item
      label="Contactmomenten"
      :data="contactmomenten"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <contactmomenten-overzicht :contactmomenten="data.page">
          <template v-slot:zaken="{ zaken }">
            <template v-for="zaakurl in zaken" :key="zaakurl">
              <zaak-preview :zaakurl="zaakurl" />
            </template>
          </template>
        </contactmomenten-overzicht>
      </template>
    </tab-list-data-item>
    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(z) => !z.count">
      <template #success="{ data }">
        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>
    <tab-list-data-item
      label="Contactverzoeken"
      :data="contactverzoeken"
      :disabled="(c) => !c.page.length"
    >
      <template #success="{ data }">
        <contactverzoeken-overzicht :contactverzoeken="data.page" />
      </template>
    </tab-list-data-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ContactmomentenOverzicht } from "@/features/contactmoment";
import {
  useBedrijfByVestigingsnummer,
  HandelsregisterGegevens,
  KlantDetails,
  useKlantById,
} from "@/features/klant";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import {
  useZakenByVestigingsnummer,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem } from "@/components/tabs";
import { useContactverzoekenByKlantId } from "@/features/contactverzoek";
import ContactverzoekenOverzicht from "@/features/contactverzoek/ContactverzoekenOverzicht.vue";

const props = defineProps<{ bedrijfId: string }>();
const klantId = computed(() => props.bedrijfId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

const currentTab = ref("");

watch(
  () => klant.success && klant.data,
  (k) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      hasContactInformation: !!k.emailadres || !!k.telefoonnummer,
    });
  },
  { immediate: true }
);

const contactverzoekenPage = ref(1);
const contactverzoeken = useContactverzoekenByKlantId(
  klantUrl,
  contactverzoekenPage
);

const contactmomentenPage = ref(1);
const contactmomenten = useContactmomentenByKlantId(
  klantUrl
  // contactmomentenPage
);

// const onContactmomentenNavigate = (page: number) => {
//   contactmomentenPage.value = page;
// };

const getVestigingsnummer = () =>
  !klant.success || !klant.data.vestigingsnummer
    ? ""
    : klant.data.vestigingsnummer;
const klantVestigingsnummer = computed(getVestigingsnummer);

const zaken = useZakenByVestigingsnummer(klantVestigingsnummer);

const bedrijf = useBedrijfByVestigingsnummer(getVestigingsnummer);
</script>
