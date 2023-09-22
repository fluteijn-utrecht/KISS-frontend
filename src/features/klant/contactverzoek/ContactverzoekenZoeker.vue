<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer of E-mailadres
      <input
        type="text"
        v-model="query"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <!-- <label class="utrecht-form-label">
      E-mailadres
      <input
        type="email"
        v-model="store.currentEmail"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label> -->
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <!-- <section class="search-section" v-if="store.klantEmail || store.klantPhone"> -->
  <section class="search-section">
    <simple-spinner v-if="zoeker.loading" />
    <template v-if="zoeker.success">
      <contactverzoeken-overzicht :records="zoeker.data">
        <template #caption>
          <SearchResultsCaption :results="zoeker.data" />
        </template>
      </contactverzoeken-overzicht>
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
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue"; //todo: spinner via slot?
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useRouter } from "vue-router";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

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
</style>
