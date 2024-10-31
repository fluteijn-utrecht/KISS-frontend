<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer of e-mailadres
      <input
        type="text"
        v-model="store.searchQuery"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <section class="search-section">
    <simple-spinner v-if="store.zoekerResults.loading" />
    <template v-if="store.zoekerResults.success">
      <table class="overview zoekresultaten-view">
        <SearchResultsCaption
          :results="filteredZoekerData"
          :zoek-termen="undefined"
        />
        <contactverzoeken-overzicht :contactverzoeken="filteredZoekerData">
          <template #onderwerp="{ contactmomentUrl }">
            <contactmoment-details-context :url="contactmomentUrl">
              <template #details="{ details }">
                {{ details?.vraag || details?.specifiekeVraag }}
              </template>
            </contactmoment-details-context>
          </template>

          <template v-if="!openKlant2" #contactmoment="{ url }">
            <contactmoment-preview :url="url">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object" />
              </template>
            </contactmoment-preview>
          </template>
        </contactverzoeken-overzicht>
      </table>
    </template>

    <application-message
      v-if="store.zoekerResults.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>
<script lang="ts" setup>
import { computed, ref, onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import { ensureState } from "@/stores/create-store";
import ContactmomentDetailsContext from "@/features/contact/contactmoment/ContactmomentDetailsContext.vue";
import ContactmomentPreview from "@/features/contact/contactmoment/ContactmomentPreview.vue";
import { useOpenKlant2 } from "@/services/openklant2";
import { search } from "./service";
import type { Contactverzoek } from "./types";
import type { PaginatedResult } from "@/services";

const openKlant2 = ref<boolean>(false);

const store = ensureState({
  stateId: "klant-zoeker",
  stateFactory() {
    return {
      searchQuery: "",
      zoekerResults: {
        loading: false,
        success: false,
        error: false,
        data: [] as PaginatedResult<Contactverzoek>[],
      },
    };
  },
});

onMounted(async () => {
  openKlant2.value = await useOpenKlant2();
});

const handleSearch = async () => {
  store.value.zoekerResults.loading = true;
  store.value.zoekerResults.success = false;
  store.value.zoekerResults.error = false;

  try {
    store.value.zoekerResults.data = await search(
      store.value.searchQuery,
      openKlant2,
    );
    store.value.zoekerResults.success = true;
  } catch (error) {
    store.value.zoekerResults.error = true;
  } finally {
    store.value.zoekerResults.loading = false;
  }
};

const filteredZoekerData = computed(() => {
  if (store.value.zoekerResults.success && store.value.searchQuery) {
    if (openKlant2.value) {
      return store.value.zoekerResults.data.flatMap((paginatedResult) =>
        paginatedResult.page.filter((item) => {
          return (
            item.record.data.betrokkene.wasPartij === null ||
            item.record.data.betrokkene.wasPartij === undefined
          );
        }),
      );
    } else {
      return store.value.zoekerResults.data.flatMap((paginatedResult) =>
        paginatedResult.page.filter(
          (item) =>
            !Object.prototype.hasOwnProperty.call(
              item.record.data.betrokkene,
              "klant",
            ),
        ),
      );
    }
  }
  return [];
});

// watch(
//   () => store.value.searchQuery,
//   async (newQuery) => {
//     if (newQuery) {
//       await handleSearch();
//     }
//   },
//   { immediate: true }
// );
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
  min-inline-size: max-content;
}

.zoekresultaten-view {
  min-inline-size: fit-content;
}
</style>
