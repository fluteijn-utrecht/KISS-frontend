<template>
  <article>
    <simple-spinner v-if="zaak.loading" />

    <application-message
      v-if="zaak.error"
      messageType="error"
      message="Er kon geen zaak gevonden worden"
    ></application-message>

    <section v-if="zaak.success">
      <div class="header">
        <utrecht-heading :level="1"
          >Zaak {{ zaak.data.identificatie }}
        </utrecht-heading>
        <router-link :to="{ name: 'zaken' }">{{
          "< Zaken zoeken"
        }}</router-link>
      </div>

      <tabs-list v-model="activeTab">
        <tabs-list-item label="Algemeen">
          <zaak-algemeen :zaak="zaak.data" />
        </tabs-list-item>
        <tabs-list-item
          label="Documenten"
          :disabled="!zaak.data.documenten?.length"
        >
          <zaak-documenten :zaak="zaak.data" />
        </tabs-list-item>
        <tabs-list-data-item
          label="Contactmomenten"
          :data="contactmomenten"
          :disabled="(c) => !c.count"
        >
          <template #success="{ data }">
            <div class="contactmomenten">
              <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
              <contactmomenten-overzicht :contactmomenten="data.page">
                <template v-slot:zaken="{ zaken }">
                  <template v-for="zaakurl in zaken" :key="zaakurl">
                    <zaak-preview :zaakurl="zaakurl"></zaak-preview>
                  </template>
                </template>
              </contactmomenten-overzicht>
            </div>
          </template>
        </tabs-list-data-item>
      </tabs-list>

      <div class="toelichting">
        <zaak-toelichting :zaak="zaak.data" @success="onNotitieUpdate" />
      </div>
    </section>
  </article>
</template>

<script setup lang="ts">
import { useZaakById } from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakToelichting from "@/features/zaaksysteem/components/ZaakToelichting.vue";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import {
  ContactmomentenOverzicht,
  useContactmomentenByObjectUrl,
} from "@/features/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import TabsList from "@/components/tabs/TabsList.vue";
import TabsListItem from "@/components/tabs/TabsListItem.vue";
import TabsListDataItem from "@/components/tabs/TabsListDataItem.vue";

const props = defineProps<{ zaakId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(computed(() => props.zaakId));
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : ""
);

const contactmomenten = useContactmomentenByObjectUrl(zaakUrl);

const Tabs = {
  algemeen: "Algemeen",
  documenten: "Documenten",
  contactmomenten: "Contactmomenten",
} as const;

const activeTab = ref(Tabs.algemeen);

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag
    );
  },
  { immediate: true }
);

const onNotitieUpdate = () => zaak.refresh();
</script>

<style lang="scss" scoped>
.contactmomenten {
  > :first-child {
    margin-block-end: var(--spacing-large);
  }
}
</style>
