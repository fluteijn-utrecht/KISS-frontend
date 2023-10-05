<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer of e-mailadres
      <input
        type="text"
        v-model="query"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <!-- <section class="search-section" v-if="store.klantEmail || store.klantPhone"> -->
  <section class="search-section">
    <simple-spinner v-if="zoeker.loading" />
    <template v-if="zoeker.success">
      <table class="overview zoekresultaten-view">
        <SearchResultsCaption :results="filteredZoekerData" />

        <contactverzoeken-overzicht :contactverzoeken="filteredZoekerData">
          <template #onderwerp="{ contactmomentUrl }">
            <slot name="onderwerp" :contactmoment-url="contactmomentUrl"></slot>
          </template>
          <template #contactmoment="{ url }">
            <slot name="contactmoment" :url="url"></slot>
          </template>
        </contactverzoeken-overzicht>
      </table>
    </template>
    <application-message
      v-if="zoeker.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>

<script lang="ts" setup>
import { watch, computed, ref } from "vue";
import { useSearch } from "./service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue"; //todo: spinner via slot?
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useRouter } from "vue-router";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";

const store = ensureState({
  stateId: "klant-zoeker",
  stateFactory() {
    return {
      // currentPhone: "",
      // currentEmail: "",
      searchQuery: "",
      // klantPhone: "",
      // klantEmail: "",
    };
  },
});

const query = ref<string>("");

const q = computed(() => ({ query: store.value.searchQuery }));

const zoeker = useSearch(q);

const singleKlantId = computed(() => {
  if (zoeker.success && zoeker.data.length === 1) {
    const first = zoeker.data[0];
    if (first?._typeOfKlant === "klant") {
      return first.id;
    }
  }
  return undefined;
});

const filteredZoekerData = computed(() => {
  if (zoeker.success) {
    return zoeker.data.filter(
      (item) =>
        !item.record.data.betrokkene.hasOwnProperty.call(
          item.record.data.betrokkene,
          "klant",
        ),
    );
  }
  return [];
});

const router = useRouter();

watch(singleKlantId, (newId, oldId) => {
  if (newId && newId !== oldId) {
    router.push(`/contacten/${newId}`);
  }
});

const handleSearch = () => {
  store.value.searchQuery = query.value;
  // store.value.klantEmail = store.value.currentEmail;
  // store.value.klantPhone = store.value.currentPhone;
};
</script>

<style lang="scss" scoped>
form {
  display: grid;
  inline-size: 30rem;
  max-inline-size: 100%;
  gap: var(--spacing-default);

  :deep(button) {
    justify-self: flex-end;
  }
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
}

.overview {
  inline-size: 30rem;
  min-inline-size: max-content;
}

.zoekresultaten-view {
  min-inline-size: fit-content;
}
</style>
