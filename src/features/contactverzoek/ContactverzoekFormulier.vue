<template>
  <SimpleSpinner v-if="afdelingen.loading" />
  <form class="container" @submit.prevent>
    <utrecht-heading :level="2">Contactverzoek maken</utrecht-heading>
    <label
      class="utrecht-form-label"
      v-if="afdelingen.success && afdelingen.data.length"
    >
      Afdeling
      <select
        v-model="afdeling"
        name="afdeling"
        class="utrecht-select utrecht-select--html-select"
      >
        <option
          v-for="afdeling in afdelingen.data"
          :key="afdeling.id"
          :value="afdeling.name"
        >
          {{ afdeling.name }}
        </option>
      </select>
    </label>

    <label class="utrecht-form-label">
      <span class="required">Contactverzoek versturen naar</span>
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="medewerker"
        :defaultValue="medewerker"
      />
    </label>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Notitie bij het contactverzoek</span>
      <textarea
        v-model="notitie"
        name="Notitie"
              aria-label="Notitie"
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="10"
      />
    </label>

    <label class="utrecht-form-label">
      <span class="required">Naam</span>
      <input
        v-model="naam"
        type="tel"
              name="Naam"
              aria-label="Naam"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>

    <label class="utrecht-form-label">
      <span class="required">Telefoonnummer 1</span>
      <input
        v-model="telefoonnummer1"
        type="tel"
              name="Telefoonnummer 1"
              aria-label="Telefoonnummer 1"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>

    <label class="utrecht-form-label">
      <span class="required">Telefoonnummer 2</span>
      <input
        v-model="telefoonnummer2"
        type="tel"
              name="Telefoonnummer 2"
              aria-label="Telefoonnummer 2"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>

    <label class="utrecht-form-label">
      <span class="required">Omschrijving telefoonnummer 2</span>
      <input
        v-model="omschrijvingTelefoonnummer2"
  
              name="Omschrijving telefoonnummer 2"
              aria-label="Omschrijving telefoonnummer 2"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>

    <label class="utrecht-form-label">
      <span class="required">E-mailadres</span>
      <input
        v-model="emailadres"        
        type="email"
              name="E-mailadres"
              aria-label="E-mailadres"
              class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Interne toelichting voor medewerker</span>
      <textarea
        v-model="interneToelichting"
        name="E-mailadres"
              aria-label="E-mailadres"
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="10"
      />
    </label>

  </form>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { useContactmomentStore, type Vraag } from "@/stores/contactmoment";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import MedewerkerSearch from "@/features/search/MedewerkerSearch.vue";
import { computed } from "vue";
import { useAfdelingen } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const props = defineProps<{
  huidigeVraag: Vraag;
}>();

const contactmomentStore = useContactmomentStore();

const medewerker = computed({
  get: () => props.huidigeVraag.contactverzoek.medewerker,
  set(medewerker) {
    if (!medewerker) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      medewerker,
    });
  },
});

const notitie = computed({
  get: () => props.huidigeVraag.contactverzoek.notitie,
  set: (notitie) => {
    if (!notitie) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      notitie,
    });
  },
});

const afdeling = computed({
  get: () => props.huidigeVraag.contactverzoek.afdeling,
  set: (afdeling) => {
    if (!afdeling) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      afdeling,
    });
  },
});

const naam = computed({
  get: () => props.huidigeVraag.contactverzoek.naam,
  set: (naam) => {
    if (!naam) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      naam,
    });
  },
});

const telefoonnummer1 = computed({
  get: () => props.huidigeVraag.contactverzoek.telefoonnummer1,
  set: (telefoonnummer1) => {
    if (!telefoonnummer1) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      telefoonnummer1,
    });
  },
});


const telefoonnummer2 = computed({
  get: () => props.huidigeVraag.contactverzoek.telefoonnummer2,
  set: (telefoonnummer2) => {
    if (!telefoonnummer2) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      telefoonnummer2,
    });
  },
});

const omschrijvingTelefoonnummer2 = computed({
  get: () => props.huidigeVraag.contactverzoek.omschrijvingTelefoonnummer2,
  set: (omschrijvingTelefoonnummer2) => {
    if (!omschrijvingTelefoonnummer2) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      omschrijvingTelefoonnummer2,
    });
  },
});

const emailadres = computed({
  get: () => props.huidigeVraag.contactverzoek.emailadres,
  set: (emailadres) => {
    if (!emailadres) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      emailadres,
    });
  },
});

const interneToelichting = computed({
  get: () => props.huidigeVraag.contactverzoek.interneToelichting,
  set: (interneToelichting) => {
    if (!interneToelichting) return;

    contactmomentStore.updateContactverzoek({
      ...props.huidigeVraag.contactverzoek,
      interneToelichting,
    });
  },
});


const afdelingen = useAfdelingen();
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-large);
}

textarea {
  resize: none;
}
</style>
