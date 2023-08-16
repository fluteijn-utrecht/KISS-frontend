<template>
  <section>
    <utrecht-heading :level="1">Contactinformatie</utrecht-heading>
    <nav>
      <ul>
        <li>
          <router-link :to="{ name: 'contacten' }">{{
            "< Contacten zoeken"
          }}</router-link>
        </li>
      </ul>
    </nav>
    <simple-spinner v-if="klant.loading" />
    <klant-details v-else-if="klant.success" :klant="klant.data" />
    <application-message
      v-if="klant.error"
      message="Er ging iets mis bij het ophalen van de klant. Probeer het later
      nog eens."
      messageType="error"
    />
    <simple-spinner v-if="contactverzoeken.loading" />
    <application-message
      v-if="contactverzoeken.error"
      message="Er ging iets mis bij het ophalen van de contactverzoeken. Probeer het later nog eens."
      messageType="error"
    />
    <template
      v-if="contactverzoeken.success && contactverzoeken.data.page.length"
    >
      <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

      <contactverzoeken-overzicht
        :contactverzoeken="contactverzoeken.data.page"
      >
        <template #contactmoment="{ url }">
          <contactmoment-preview :url="url">
            <template #object="{ object }">
              <zaak-preview :zaakurl="object.object" />
            </template>
          </contactmoment-preview>
        </template>
      </contactverzoeken-overzicht>
    </template>

    <!-- Contactmomenten -->
    <simple-spinner v-if="contactmomenten.loading" />

    <application-message
      v-if="contactmomenten.error"
      message="Er ging iets mis bij het ophalen van de contactmomenten. Probeer het later nog eens."
      messageType="error"
    />

    <template v-if="contactmomenten.success && contactmomenten.data">
      <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

      <contactmomenten-overzicht :contactmomenten="contactmomenten.data.page">
      </contactmomenten-overzicht>
      <!-- 
      <pagination
        class="pagination"
        :pagination="contactmomenten.data"
        @navigate="onContactmomentenNavigate"
      /> -->
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ContactmomentenOverzicht } from "@/features/contactmoment";
import { useKlantById, KlantDetails } from "@/features/klant";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import { useContactverzoekenByKlantId } from "@/features/contactverzoek";
import ContactverzoekenOverzicht from "@/features/contactverzoek/ContactverzoekenOverzicht.vue";
import ContactmomentPreview from "@/features/contactmoment/ContactmomentPreview.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
const props = defineProps<{ contactId: string }>();
const klantId = computed(() => props.contactId);
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
  klantUrl,
  contactverzoekenPage
);

// const contactmomentenPage = ref(1);
const contactmomenten = useContactmomentenByKlantId(
  klantUrl
  // contactmomentenPage
);

// const onContactmomentenNavigate = (page: number) => {
//   contactmomentenPage.value = page;
// };
</script>

<style scoped lang="scss">
nav {
  list-style: none;
}

section > * {
  margin-block-end: var(--spacing-large);
}

utrecht-heading {
  margin-block-end: 0;
}
</style>
