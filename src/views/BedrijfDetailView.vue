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

  <data-tabs :state="state">
    <template #[tabs.contactgegevens]="{ data }">
      <bedrijf-details :klant="data" />
    </template>
    <template #[tabs.kvk]="{ data }">
      <handelsregister-gegevens v-if="data" :bedrijf="data" />
    </template>
    <template #[tabs.contactmomenten]="{ data }">
      <contactmomenten-overzicht :contactmomenten="data.page">
        <template v-slot:zaken="{ zaken }">
          <template v-for="zaakurl in zaken" :key="zaakurl">
            <zaak-preview :zaakurl="zaakurl" />
          </template>
        </template>
      </contactmomenten-overzicht>
    </template>
    <template #[tabs.zaken]="{ data }">
      <zaken-overzicht
        :zaken="data.page"
        :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
      />
    </template>
    <template #[tabs.contactverzoeken]="{ data }">
      <contactverzoeken-overzicht :contactverzoeken="data.page" />
    </template>
  </data-tabs>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import {
  ContactmomentenOverzicht,
  useContactverzoekenByKlantId,
} from "@/features/contactmoment";
import {
  useBedrijfByVestigingsnummer,
  HandelsregisterGegevens,
  BedrijfDetails,
  useKlantById,
} from "@/features/klant";
import ContactverzoekenOverzicht from "@/features/contactmoment/ContactverzoekenOverzicht.vue";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import {
  useZakenByVestigingsnummer,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import DataTabs, {
  tabStateValue,
  type TabState,
} from "@/components/DataTabs.vue";
const props = defineProps<{ bedrijfId: string }>();
const klantId = computed(() => props.bedrijfId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

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
  klantId,
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

const tabs = {
  contactgegevens: "Contactgegevens",
  kvk: "KvK-gegevens",
  contactmomenten: "Contactmomenten",
  zaken: "Zaken",
  contactverzoeken: "Contactverzoeken",
} as const;

type Tabs = typeof tabs;
type Tab = Tabs[keyof Tabs];

const state = {
  [tabs.contactgegevens]: tabStateValue(klant, (k) => !!k),
  [tabs.kvk]: tabStateValue(bedrijf, (b) => !!b),
  [tabs.contactmomenten]: tabStateValue(contactmomenten, (c) => !!c.count),
  [tabs.zaken]: tabStateValue(zaken, (z) => !!z.count),
  [tabs.contactverzoeken]: tabStateValue(
    contactverzoeken,
    (c) => !!c.page.length
  ),
} satisfies TabState<Tab>;
</script>
