<template>
  <back-link />

  <utrecht-heading :level="1">Persoonsinformatie</utrecht-heading>

  <tab-list v-model="activeTab">
    <tab-list-data-item label="Contactgegevens" :data="klant">
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>

    <tab-list-data-item label="BRP gegevens" :data="persoon">
      <template #success="{ data }">
        <brp-gegevens v-if="data" :persoon="data" />
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
          <template v-slot:object="{ object }">
            <zaak-preview :zaakurl="object.object"></zaak-preview>
          </template>
        </contactmomenten-overzicht>
      </template>
    </tab-list-data-item>

    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(c) => !c.count">
      <template #success="{ data }">
        <utrecht-heading :level="2"> Zaken </utrecht-heading>

        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
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

    <tab-list-data-item
      label="Notificaties"
      :data="notificaties"
      :disabled="(c: PaginatedResult<ContactmomentViewModel>) => !c.count"
    >
      <template #success="{ data }">
        <utrecht-heading :level="2"> Notificaties </utrecht-heading>

        <contactmomenten-overzicht :contactmomenten="data.page">
          <template v-slot:object="{ object }">
            <zaak-preview :zaakurl="object.object"></zaak-preview>
          </template>
        </contactmomenten-overzicht>
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
  KlantDetails,
  useKlantById,
  BrpGegevens,
  usePersoonByBsn,
} from "@/features/klant";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import { useZakenByBsn } from "@/features/zaaksysteem";
import ZakenOverzicht from "@/features/zaaksysteem/ZakenOverzicht.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { useContactverzoekenByKlantId } from "@/features/contactverzoek";
import ContactverzoekenOverzicht from "@/features/contactverzoek/ContactverzoekenOverzicht.vue";
import ContactmomentPreview from "@/features/contactmoment/ContactmomentPreview.vue";
import { TabList, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import type { ContactmomentViewModel } from "@/features/shared/types";
import type { PaginatedResult } from "@/services";

const activeTab = ref("");

const props = defineProps<{ persoonId: string }>();
const klantId = computed(() => props.persoonId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

const contactverzoekenPage = ref(1);
const contactverzoeken = useContactverzoekenByKlantId(
  klantUrl,
  contactverzoekenPage,
);

// const contactmomentenPage = ref(1);
const contactmomenten = useContactmomentenByKlantId(klantUrl, "klant");

const notificaties = useContactmomentenByKlantId(klantUrl, "gemeente");

// const onContactmomentenNavigate = (page: number) => {
//   contactmomentenPage.value = page;
// };

const getBsn = () => (!klant.success || !klant.data.bsn ? "" : klant.data.bsn);
const klantBsn = computed(getBsn);

const zaken = useZakenByBsn(klantBsn);
const persoon = usePersoonByBsn(getBsn);

watch(
  [() => klant.success && klant.data, () => persoon.success && persoon.data],
  ([k, p]) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      ...p,
      hasContactInformation: !!k.emailadres || !!k.telefoonnummer,
    });
  },
  { immediate: true },
);
</script>
