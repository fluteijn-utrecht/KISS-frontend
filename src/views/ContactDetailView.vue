<template>
  <back-link />

  <utrecht-heading :level="1">Contactinformatie</utrecht-heading>
  <tab-list v-model="activeTab">
    <tab-list-data-item label="Contactgegevens" :data="klant">
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>

    <tab-list-data-item
      label="Contactmomenten"
      :data="contactmomenten"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
        <contactmomenten-overzicht :contactmomenten="data.page">
        </contactmomenten-overzicht>
        <!-- 
          <pagination
            class="pagination"
            :pagination="contactmomenten.data"
            @navigate="onContactmomentenNavigate"
          /> -->
      </template>
    </tab-list-data-item>

    <tab-list-data-item
      label="Contactverzoeken"
      :data="contactverzoeken"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

        <contactverzoeken-overzicht :contactverzoeken="data.page">
          <template #contactmoment="{ url }">
            <contactmoment-preview :url="url">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object" />
              </template>
            </contactmoment-preview>
          </template>
        </contactverzoeken-overzicht>
      </template>
    </tab-list-data-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ContactmomentenOverzicht } from "@/features/contactmoment";
import { useKlantById, KlantDetails } from "@/features/klant";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import { useContactverzoekenByKlantId } from "@/features/contactverzoek";
import ContactverzoekenOverzicht from "@/features/contactverzoek/ContactverzoekenOverzicht.vue";
import ContactmomentPreview from "@/features/contactmoment/ContactmomentPreview.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";

const activeTab = ref("");

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
  { immediate: true },
);

const contactverzoekenPage = ref(1);
const contactverzoeken = useContactverzoekenByKlantId(
  klantUrl,
  contactverzoekenPage,
);

const contactmomenten = useContactmomentenByKlantId(klantUrl);
</script>
