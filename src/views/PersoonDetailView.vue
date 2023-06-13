<template>
  <section>
    <utrecht-heading :level="1">Persoonsinformatie</utrecht-heading>
    <nav>
      <ul>
        <li>
          <router-link :to="{ name: 'personen' }">{{
            "< Personen zoeken"
          }}</router-link>
        </li>
      </ul>
    </nav>
    <simple-spinner v-if="klant.loading" />
    <persoon-details v-else-if="klant.success" :klant="klant.data" />
    <application-message
      v-if="klant.error"
      message="Er ging iets mis bij het ophalen van de klant. Probeer het later
      nog eens."
      messageType="error"
    />

    <simple-spinner v-if="persoon.loading" />
    <brp-gegevens
      v-if="persoon.success && persoon.data"
      :persoon="persoon.data"
    />
    <application-message
      v-if="persoon.error"
      message="Er ging iets mis bij het ophalen van de BRP gegevens. Probeer het later nog eens."
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
      />
    </template>

    <!-- Zaken -->

    <simple-spinner v-if="zaken.loading" />

    <application-message
      v-if="zaken.error"
      message="Er ging iets mis bij het ophalen van de zaken. Probeer het later nog eens."
      messageType="error"
    />

    <template v-if="zaken.success && zaken.data.page.length">
      <utrecht-heading :level="2"> Zaken </utrecht-heading>

      <zaken-overzicht
        :zaken="zaken.data.page"
        :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
      />
    </template>

    <!-- Contactmomenten -->

    <simple-spinner v-if="contactmomenten.loading" />

    <application-message
      v-if="contactmomenten.error"
      message="Er ging iets mis bij het ophalen van de contactmomenten. Probeer het later nog eens."
      messageType="error"
    />

    <!-- [ { "url": "https://open-klant.dev.kiss-demo.nl/contactmomenten/api/v1/contactmomenten/3b8993c6-6e04-434a-9a61-af6acad88b46", "vorigContactmoment": null, "volgendContactmoment": null, "bronorganisatie": "999990639", "registratiedatum": "2023-06-08T12:23:42.467000Z", "kanaal": "Twitter", "voorkeurskanaal": "", "voorkeurstaal": "", "tekst": "asadsd....", "onderwerpLinks": [], "initiatiefnemer": "klant", "medewerker": "", "medewerkerIdentificatie": { "identificatie": "todo", "achternaam": "todo", "voorletters": "todo", "voorvoegselAchternaam": "todo" }, "klantcontactmomenten": [ "https://open-klant.dev.kiss-demo.nl/contactmomenten/api/v1/klantcontactmomenten/805c22b2-3fe0-4a4a-b4dd-dead1b025d8b" ], "objectcontactmomenten": [ "https://open-klant.dev.kiss-demo.nl/contactmomenten/api/v1/objectcontactmomenten/56dc0a99-29aa-447d-899f-aa1590536803" ] } ] -->

    <template v-if="contactmomenten.success && contactmomenten.data">
      <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

      <contactmomenten-overzicht :contactmomenten="contactmomenten.data" />

      <pagination
        class="pagination"
        :pagination="contactmomenten.data"
        @navigate="onContactmomentenNavigate"
      />
    </template>
  </section>
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
  PersoonDetails,
  useKlantById,
  BrpGegevens,
  usePersoonByBsn,
} from "@/features/klant";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ContactverzoekenOverzicht from "@/features/contactmoment/ContactverzoekenOverzicht.vue";
import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/shared/get-contactmomenten-service";
import { useZakenByBsn } from "@/features/zaaksysteem";
import ZakenOverzicht from "@/features/zaaksysteem/ZakenOverzicht.vue";

const props = defineProps<{ persoonId: string }>();
const klantId = computed(() => props.persoonId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);

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
  klantId,
  contactmomentenPage
);

const onContactmomentenNavigate = (page: number) => {
  contactmomentenPage.value = page;
};

const getBsn = () => (!klant.success || !klant.data.bsn ? "" : klant.data.bsn);
const klantBsn = computed(getBsn);

const zaken = useZakenByBsn(klantBsn);
const persoon = usePersoonByBsn(getBsn);
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
