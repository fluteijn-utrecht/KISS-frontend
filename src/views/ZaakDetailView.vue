<template>
  <simple-spinner v-if="zaak.loading" />

  <application-message
    v-if="zaak.error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak.success">
    <back-link />

    <header class="zaak-header">
      <utrecht-heading :level="1"
        >Zaak {{ zaak.data.identificatie }}</utrecht-heading
      >
      <zaak-deeplink :zaak="zaak.data" />
    </header>

    <tab-list v-model="activeTab" v-if="zaaksysteemId">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-item
        label="Documenten"
        :disabled="!zaak.data.documenten?.length"
      >
        <zaak-documenten :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-item
        label="Contactmomenten"
        :disabled="!data?.count"
        class="data-tabpanel"
      >
        <template #tab="{ isActive, label }">
          <div class="data-tab" :data-active="isActive">
            <span :class="error ? 'alert icon-after' : ''">{{ label }}</span>
            <simple-spinner class="small-spinner" v-if="loading" />
          </div>
        </template>
        <template #default="{ isActive }">
          <div class="data-tabpanel" :data-active="isActive">
            <div class="contactmomenten">
              <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
              <contactmomenten-overzicht :contactmomenten="data?.page || []">
                <template #object="{ object }">
                  <zaak-preview :zaakurl="object.object"></zaak-preview>
                </template>
              </contactmomenten-overzicht>
            </div>
          </div>
        </template>
      </tab-list-item>
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
  fetchContactmomentenByObjectUrl as fetchContactmomentenByObjectUrl,
} from "@/features/contact/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import ZaakDeeplink from "@/features/zaaksysteem/components/ZaakDeeplink.vue";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import type { PaginatedResult } from "@/services";
import type { ContactmomentViewModel } from "@/services/openklant2";

const props = defineProps<{ zaakId: string; zaaksysteemId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(
  computed(() => props.zaakId),
  computed(() => props.zaaksysteemId),
);
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : "",
);

const data = ref<PaginatedResult<ContactmomentViewModel>>();
const loading = ref(true);
const error = ref(false);
watch(
  [zaakUrl],
  ([u]) => {
    if (!u) return;
    loading.value = true;
    fetchContactmomentenByObjectUrl(u, true)
      .then((r) => {
        data.value = r;
      })
      .catch(() => {
        error.value = true;
      })
      .finally(() => {
        loading.value = false;
      });
  },
  { immediate: true },
);

const activeTab = ref("");

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
      true,
      props.zaaksysteemId,
    );
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped>
.zaak-header {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-default);
  align-items: center;
}
</style>
