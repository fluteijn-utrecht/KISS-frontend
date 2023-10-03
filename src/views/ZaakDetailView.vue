<template>
  <simple-spinner v-if="zaak.loading" />

  <application-message
    v-if="zaak.error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak.success">
    <back-link />

    <utrecht-heading :level="1"
      >Zaak {{ zaak.data.identificatie }}</utrecht-heading
    >

    <tab-list v-model="activeTab">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-item
        label="Documenten"
        :disabled="!zaak.data.documenten?.length"
      >
        <zaak-documenten :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-data-item
        label="Contactmomenten"
        :data="contactmomenten"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <div class="contactmomenten">
            <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
            <contactmomenten-overzicht :contactmomenten="data.page">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object"></zaak-preview>
              </template>
            </contactmomenten-overzicht>
          </div>
        </template>
      </tab-list-data-item>
      <tab-list-data-item
        label="Notificaties"
        :data="notificaties"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <utrecht-heading :level="2"> Notificaties </utrecht-heading>

          <notificaties-overzicht :notificaties="data.page">
            <template v-slot:object="{ object }">
              <zaak-preview :zaakurl="object.object"></zaak-preview>
            </template>
          </notificaties-overzicht>
        </template>
      </tab-list-data-item>
    </tab-list>
  </template>
</template>

<script setup lang="ts">
import { useZaakById } from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import {
  ContactmomentenOverzicht,
  NotificatiesOverzicht,
  useContactmomentenByObjectUrl,
} from "@/features/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListItem, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
const props = defineProps<{ zaakId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(computed(() => props.zaakId));
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : "",
);

const contactmomenten = useContactmomentenByObjectUrl(zaakUrl, "klant");
const notificaties = useContactmomentenByObjectUrl(zaakUrl, "gemeente");

const activeTab = ref("");

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
    );
  },
  { immediate: true },
);
</script>
