<template>
  <section>
    <form class="search-bar" @submit.prevent="zoekOpZaak">
      <label>
        <input
          type="search"
          placeholder="Zoek op zaaknummer"
          v-model="store.searchField"
          title="ZAAK-1"
        />
      </label>
      <button title="Zoeken">
        <span>Zoeken</span>
      </button>
    </form>

    <template v-if="store.currentSearch">
      <application-message
        v-if="zaken.error"
        messageType="error"
        message="Er is een probleem opgetreden."
      ></application-message>
      <simple-spinner v-else-if="zaken.loading"></simple-spinner>
      <section class="resultaten-container" v-if="zaken.success">
        <zaken-overzicht
          :zaken="zaken.data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        >
          <template #caption>
            <SearchResultsCaption :results="zaken.data" />
          </template>
        </zaken-overzicht>
      </section>
    </template>
  </section>
</template>

<script lang="ts" setup>
import { watch, computed } from "vue";
import { useZakenByZaaknummer } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import ZakenOverzicht from "./ZakenOverzicht.vue";
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useContactmomentStore } from "@/stores/contactmoment";
import { useRouter } from "vue-router";
import SearchResultsCaption from "../../components/SearchResultsCaption.vue";

const contactmomentStore = useContactmomentStore();

const store = ensureState({
  stateId: "zaak-zoeker",
  stateFactory() {
    return {
      searchField: "",
      currentSearch: "",
    };
  },
});

const zaken = useZakenByZaaknummer(computed(() => store.value.currentSearch));

const zoekOpZaak = () => {
  store.value.currentSearch = store.value.searchField;
};

const singleZaakId = computed(() =>
  zaken.success && zaken.data.page.length === 1
    ? zaken.data.page[0].id
    : undefined
);

const router = useRouter();

watch(singleZaakId, (newId, oldId) => {
  if (newId && newId !== oldId) {
    router.push(`/zaken/${newId}`);
  }
});
</script>

<style lang="scss" scoped>
.search-bar {
  inline-size: min(100%, 20rem);
  margin-block-end: var(--spacing-large);
}
</style>
