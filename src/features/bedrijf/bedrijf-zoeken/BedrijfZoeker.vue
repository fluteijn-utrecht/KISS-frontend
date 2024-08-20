<template>
  <form @submit.prevent="handleSearch">
    <fieldset class="radio-group">
      <legend>Waar wil je op zoeken?</legend>
      <label v-for="(label, field) in labels" :key="field">
        <input type="radio" :value="field" v-model="state.field" required />
        {{ label }}
      </label>
    </fieldset>
    <fieldset class="search-bar">
      <label>
        <span>Zoek naar een bedrijf</span>
        <input
          type="search"
          placeholder="Zoek naar een bedrijf"
          ref="inputRef"
          v-model="state.currentSearch"
          @search="handleSearch"
        />
      </label>
      <button title="Zoeken">
        <span>Zoeken</span>
      </button>
    </fieldset>
  </form>

  <section class="search-section" v-if="state.query">
    <simple-spinner v-if="bedrijven.loading" />
    <template v-if="bedrijven.success">
      <bedrijven-overzicht
        :records="bedrijven.data.page"
        :navigate-on-single-result="navigateOnSingleResult"
      >
        <template #caption v-if="'pageNumber' in bedrijven.data">
          <SearchResultsCaption :results="bedrijven.data" />
        </template>
      </bedrijven-overzicht>
      <pagination
        v-if="'pageNumber' in bedrijven.data"
        class="pagination"
        :pagination="bedrijven.data"
        @navigate="navigate"
      />
    </template>
    <application-message
      v-if="bedrijven.error"
      messageType="error"
      :message="
        bedrijven.error instanceof FriendlyError
          ? bedrijven.error.message
          : 'Er is een fout opgetreden'
      "
    />
  </section>
</template>

<script setup lang="ts">
import {
  parseKvkNummer,
  parsePostcodeHuisnummer,
  type PostcodeHuisnummer,
} from "@/helpers/validation";
import { ensureState } from "@/stores/create-store";
import { computed, ref, watch } from "vue";
import { useSearchBedrijven } from "./use-search-bedrijven";

import SimpleSpinner from "@/components/SimpleSpinner.vue";
import Pagination from "@/nl-design-system/components/Pagination.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import BedrijvenOverzicht from "./BedrijvenOverzicht.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { FriendlyError } from "@/services";

const labels = {
  handelsnaam: "Bedrijfsnaam",
  kvkNummer: "KVK-nummer",
  postcodeHuisnummer: "Postcode + Huisnummer",
  email: "E-mailadres",
  telefoonnummer: "Telefoonnummer",
} as const;

type SearchOptions =
  | { email: string }
  | { telefoonnummer: string }
  | { handelsnaam: string }
  | { kvkNummer: string }
  | { postcodeHuisnummer: PostcodeHuisnummer };

type SearchFields = keyof typeof labels;

const state = ensureState({
  stateId: "BedrijfZoeker",
  stateFactory() {
    return {
      currentSearch: "",
      field: Object.keys(labels)[0] as SearchFields,
      query: undefined as SearchOptions | undefined,
      page: 1,
    };
  },
});

const inputRef = ref();

const currentQuery = computed<SearchOptions | { error: Error }>(() => {
  const { currentSearch, field } = state.value;

  if (field === "postcodeHuisnummer") {
    const parsed = parsePostcodeHuisnummer(currentSearch);
    return parsed instanceof Error
      ? {
          error: parsed,
        }
      : {
          postcodeHuisnummer: parsed,
        };
  }

  if (field === "kvkNummer") {
    const parsed = parseKvkNummer(currentSearch);
    return parsed instanceof Error
      ? {
          error: parsed,
        }
      : {
          kvkNummer: parsed,
        };
  }

  return {
    [field]: currentSearch,
  } as SearchOptions;
});

const navigateOnSingleResult = ref(false);

watch(
  [currentQuery, inputRef],
  ([query, input]) => {
    if (!(input instanceof HTMLInputElement)) return;
    input.setCustomValidity(
      query && "error" in query ? query.error.message : "",
    );
  },
  { immediate: true },
);

const handleSearch = () => {
  const query = currentQuery.value;
  if (!query || "error" in query) return;
  state.value.query = query;
  state.value.page = 1;
  navigateOnSingleResult.value = true;
};

const bedrijven = useSearchBedrijven(() => state.value.query);

const navigate = (val: number) => {
  state.value.page = val;
};
</script>

<style scoped lang="scss">
input[type="search"] {
  width: var(--section-width);
}

.search-bar {
  margin-bottom: var(--spacing-large);
  width: min(100%, 20rem);
}

.pagination {
  margin-inline: auto;
}

.search-section {
  display: grid;
  gap: var(--spacing-small);
}

input[type="radio"] {
  accent-color: var(--color-primary);
  margin: 0;
}

.radio-group {
  display: grid;
  grid-template-columns: repeat(3, auto);
  justify-content: flex-start;
  grid-gap: var(--spacing-extrasmall) var(--spacing-default);

  > legend {
    font-size: 0;
  }

  label {
    display: flex;
    gap: var(--spacing-small);
    align-items: center;
  }
}
</style>
