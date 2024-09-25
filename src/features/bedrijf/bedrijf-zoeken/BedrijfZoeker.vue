<template>
  <section class="actions">
    <form @submit.prevent="zoekOpBedrijfsnaam" class="zoekerForm">
      <label class="utrecht-form-label">
        Bedrijfsnaam
        <input
          v-validate="store.bedrijfsnaam"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>

    <form @submit.prevent="zoekOpKvkOfVestigingsnummer" class="zoekerForm">
      <label class="utrecht-form-label">
        KVK-nummer of vestigingsnummer
        <input
          v-validate="store.kvkOfVestigingsnummer"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>

      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>

    <form @submit.prevent="zoekOpPostcodeHuisnummer" class="zoekerForm">
      <label class="utrecht-form-label">
        Postcode
        <input
          v-validate="store.postcode"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <label class="utrecht-form-label">
        Huisnummer
        <input
          v-validate="store.huisnummer"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>

    <!-- <form @submit.prevent="handleSearch" class="zoekerForm">
    <fieldset class="radio-group">
      <legend>Waar wil je op zoeken?</legend>
      <label v-for="(label, field) in labels" :key="field">
        <input type="radio" :value="field" v-model="store.field" required />
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
          v-model="store.currentSearch"
          @search="handleSearch"
        />
      </label>
      <button title="Zoeken">
        <span>Zoeken</span>
      </button>
    </fieldset>
  </form> -->
  </section>

  <section class="search-section" v-if="store.query">
    <simple-spinner v-if="bedrijven.loading" />
    <template v-if="bedrijven.success">
      <bedrijven-overzicht
        :records="bedrijven.data.page"
        :navigate-on-single-result="navigateOnSingleResult"
      >
        <template #caption v-if="'pageNumber' in bedrijven.data">
          <SearchResultsCaption
            :results="bedrijven.data"
            :zoekTermen="store.query"
          />
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
  vValidate,
  parseBedrijfsnaam,
  parseKkvkOfVestigingsnummer,
  parsePostcodeHuisnummer,
  validateWith,
  parsePostcode,
  parseHuisnummer,
} from "@/helpers/validation";
import { ensureState } from "@/stores/create-store";
import { computed, ref, watch } from "vue";
import {
  useSearchBedrijven,
  type BedrijvenQuery,
} from "./use-search-bedrijven";

import SimpleSpinner from "@/components/SimpleSpinner.vue";
import Pagination from "@/nl-design-system/components/Pagination.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import BedrijvenOverzicht from "./BedrijvenOverzicht.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { FriendlyError } from "@/services";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const labels = {
  handelsnaam: "Bedrijfsnaam",
  kvkNummer: "KVK-nummer",
  postcodeHuisnummer: "Postcode + Huisnummer",
  email: "E-mailadres",
  telefoonnummer: "Telefoonnummer",
};

type SearchFields = keyof typeof labels;

const store = ensureState({
  stateId: "BedrijfZoeker",
  stateFactory() {
    return {
      currentSearch: "",

      bedrijfsnaam: validateWith(parseBedrijfsnaam),
      kvkOfVestigingsnummer: validateWith(parseKkvkOfVestigingsnummer),
      postcode: validateWith(parsePostcode),
      huisnummer: validateWith(parseHuisnummer),

      field: Object.keys(labels)[0] as SearchFields,
      query: undefined as BedrijvenQuery | undefined,
      page: 1,
    };
  },
});

// const inputRef = ref();

// const currentQuery = computed<BedrijvenQuery | { error: Error } | undefined>(
//   () => {
//     const { currentSearch, field } = store.value;

//     if (!currentSearch) return undefined;

//     if (field === "postcodeHuisnummer") {
//       const parsed = parsePostcodeHuisnummer(currentSearch);
//       return parsed instanceof Error
//         ? {
//             error: parsed,
//           }
//         : {
//             postcodeHuisnummer: parsed,
//           };
//     }

//     // if (field === "kvkNummer") {
//     //   const parsed = parseKvkNummer(currentSearch);
//     //   return parsed instanceof Error
//     //     ? {
//     //         error: parsed,
//     //       }
//     //     : {
//     //         kvkNummer: parsed,
//     //       };
//     // }

//     if (field === "handelsnaam") {
//       return {
//         [field]: currentSearch,
//       };
//     }

//     if (field === "email") {
//       return {
//         email: currentSearch,
//       };
//     }

//     if (field === "telefoonnummer") {
//       return {
//         email: currentSearch,
//       };
//     }

//     return undefined;
//   },
// );

// watch(
//   [currentQuery, inputRef],
//   ([query, input]) => {
//     if (!(input instanceof HTMLInputElement)) return;
//     input.setCustomValidity(
//       query && "error" in query ? query.error.message : "",
//     );
//   },
//   { immediate: true },
// );

// const handleSearch = () => {
//   const query = currentQuery.value;
//   if (!query || "error" in query) return;
//   state.value.query = query;
//   state.value.page = 1;
//   navigateOnSingleResult.value = true;
// };

const navigateOnSingleResult = ref(false);

const zoekOpKvkOfVestigingsnummer = () => {
  if (store.value.kvkOfVestigingsnummer.validated) {
    store.value.query = {
      kvkNummer: store.value.kvkOfVestigingsnummer.validated,
    }; //todo ook vestigingsnummer zoeken...
    store.value.page = 1;
    navigateOnSingleResult.value = true;
  }
};

const zoekOpBedrijfsnaam = () => {
  if (store.value.bedrijfsnaam.validated) {
    (store.value.query = {
      handelsnaam: store.value.bedrijfsnaam.validated,
    }),
      (store.value.page = 1);
    navigateOnSingleResult.value = true;
  }
};

const zoekOpPostcodeHuisnummer = () => {
  if (store.value.postcode.validated && store.value.huisnummer.validated) {
    store.value.query = {
      postcodeHuisnummer: {
        postcode: store.value.postcode.validated,
        huisnummer: store.value.huisnummer.validated,
      },
    };
    navigateOnSingleResult.value = true;
  }
};

const bedrijven = useSearchBedrijven(
  () =>
    store.value.query && {
      query: store.value.query,
      page: store.value.page,
    },
);

const navigate = (val: number) => {
  store.value.page = val;
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

// todo kopie van persoonzoker. generiek maken
.actions {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
  inline-size: min(40rem, 100%);
}
</style>
