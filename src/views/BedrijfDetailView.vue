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
  <tabs-component class="detail-tabs" v-model="currentTab">
    <template #[tabs.contactgegevens]>
      <simple-spinner v-if="klant.loading" />
      <bedrijf-details v-else-if="klant.success" :klant="klant.data" />
      <application-message
        v-if="klant.error"
        message="Er ging iets mis bij het ophalen van de klant. Probeer het later
      nog eens."
        messageType="error"
      />
    </template>
    <template #[tabs.kvk]>
      <simple-spinner v-if="bedrijf.loading" />
      <handelsregister-gegevens
        v-if="bedrijf.success && bedrijf.data"
        :bedrijf="bedrijf.data"
      />
      <application-message
        v-if="bedrijf.error"
        message="Er ging iets mis bij het ophalen van de Handelsregister gegevens. Probeer het later nog eens."
        messageType="error"
      />
    </template>
    <template #[tabs.contactmomenten]>
      <utrecht-heading :level="2"> Contactmomenten</utrecht-heading>
      <simple-spinner v-if="contactmomenten.loading" />
      <application-message
        v-if="contactmomenten.error"
        message="Er ging iets mis bij het ophalen van de contactmomenten. Probeer het later nog eens."
        messageType="error"
      />
      <template v-if="contactmomenten.success && contactmomenten.data">
        <contactmomenten-overzicht :contactmomenten="contactmomenten.data.page">
          <template v-slot:zaken="{ zaken }">
            <template v-for="zaakurl in zaken" :key="zaakurl">
              <zaak-preview :zaakurl="zaakurl"></zaak-preview>
            </template>
          </template>
        </contactmomenten-overzicht>
        <!--
  <pagination
    class="pagination"
    :pagination="contactmomenten.data"
    @navigate="onContactmomentenNavigate"
  /> -->
      </template>
    </template>
    <template #[tabs.zaken]>
      <utrecht-heading :level="2"> Zaken </utrecht-heading>
      <simple-spinner v-if="zaken.loading" />
      <application-message
        v-if="zaken.error"
        message="Er ging iets mis bij het ophalen van de zaken. Probeer het later nog eens."
        messageType="error"
      />
      <template v-if="zaken.success">
        <zaken-overzicht
          v-if="zaken.data.page.length"
          :zaken="zaken.data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
        <p v-else>Geen zaken gevonden.</p>
      </template>
    </template>
    <template #[tabs.contactverzoeken]>
      <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>
      <simple-spinner v-if="contactverzoeken.loading" />
      <application-message
        v-if="contactverzoeken.error"
        message="Er ging iets mis bij het ophalen van de contactverzoeken. Probeer het later nog eens."
        messageType="error"
      />
      <template
        v-if="contactverzoeken.success && contactverzoeken.data.page.length"
      >
        <contactverzoeken-overzicht
          :contactverzoeken="contactverzoeken.data.page"
        />
      </template>
    </template>
  </tabs-component>
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
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ContactverzoekenOverzicht from "@/features/contactmoment/ContactverzoekenOverzicht.vue";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import {
  useZakenByVestigingsnummer,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import TabsComponent from "@/components/TabsComponent.vue";

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

const currentTab = ref<Tab>(tabs.contactgegevens);
</script>
