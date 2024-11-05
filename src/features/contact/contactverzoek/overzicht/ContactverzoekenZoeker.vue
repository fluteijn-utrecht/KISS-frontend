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
          :results="store.zoekerResults.data"
          :zoek-termen="undefined"
        />
        <contactverzoeken-overzicht
          :contactverzoeken="store.zoekerResults.data"
        >
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
import { ref, onMounted } from "vue";
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
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

const openKlant2 = ref<boolean>(false);

const store = ensureState({
  stateId: "contactverzoeken-zoeker",
  stateFactory() {
    return {
      searchQuery: "",
      zoekerResults: {
        loading: false,
        success: false,
        error: false,
        data: [] as Contactverzoek[],
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
      openKlant2.value,
    );
    store.value.zoekerResults.success = true;
  } catch (error) {
    store.value.zoekerResults.error = true;
  } finally {
    store.value.zoekerResults.loading = false;
  }
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
  min-inline-size: max-content;
}

.zoekresultaten-view {
  min-inline-size: fit-content;
}
</style>
