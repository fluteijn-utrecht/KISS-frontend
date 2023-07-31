<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer
      <input
        type="tel"
        v-model="store.currentPhone"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <label class="utrecht-form-label">
      E-mailadres
      <input
        type="email"
        v-model="store.currentEmail"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <section class="search-section" v-if="store.klantEmail || store.klantPhone">
    <simple-spinner v-if="klanten.loading" />
    <template v-if="klanten.success">
      <contacten-overzicht :records="klanten.data">
        <template #caption>
          <SearchResultsCaption :results="klanten.data" />
        </template>
      </contacten-overzicht>
    </template>
    <application-message
      v-if="klanten.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>

<script lang="ts" setup>
import { watch, computed } from "vue";
import { useSearchKlanten } from "./service";
import ContactenOverzicht from "./ContactenOverzicht.vue";
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
      currentPhone: "",
      currentEmail: "",
      klantPhone: "",
      klantEmail: "",
    };
  },
});

const klanten = useSearchKlanten(
  computed(() => ({
    email: store.value.klantEmail,
    phone: store.value.klantPhone,
  }))
);

const singleKlantId = computed(() => {
  if (klanten.success && klanten.data.length === 1) {
    const first = klanten.data[0];
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
  store.value.klantEmail = store.value.currentEmail;
  store.value.klantPhone = store.value.currentPhone;
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
