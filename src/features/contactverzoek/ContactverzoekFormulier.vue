<template>
  <div class="container" @submit.prevent>
    <label class="utrecht-form-label">
      <span class="required">Contactverzoek maken voor</span>
      <select
        required
        v-model="form.isMedewerker"
        @change="setActive"
        name="isMedewerker"
        class="utrecht-select utrecht-select--html-select"
      >
        <option :value="undefined">Afdeling</option>
        <option :value="true">Medewerker</option>
      </select>
    </label>

    <label class="utrecht-form-label" v-if="form.isMedewerker">
      <span class="required">Contactverzoek versturen naar</span>
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        v-model="form.medewerker"
        @update:model-value="setActive"
      />
    </label>

    <template v-else>
      <label class="utrecht-form-label">
        <span class="required">Afdeling</span>
        <afdeling-search
          class="utrecht-textbox utrecht-textbox--html-input"
          v-model="form.afdeling"
          :required="true"
          @update:model-value="setActive"
        />
      </label>
      <label class="utrecht-form-label" v-if="form.afdeling">
        Groep
        <groep-search
          class="utrecht-textbox utrecht-textbox--html-input"
          v-model="form.groep"
          :afdeling-id="form.afdeling.id"
          @update:model-value="setActive"
        />
      </label>
    </template>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Interne toelichting voor medewerker</span>
      <textarea
        v-model="form.interneToelichting"
        name="Interne toelichting"
        required
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="5"
        @input="setActive"
      />
    </label>

    <form-fieldset>
      <form-fieldset-legend>Contact opnemen met</form-fieldset-legend>
      <label class="utrecht-form-label">
        <span>Voornaam</span>
        <input
          v-model="form.voornaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Tussenvoegsel</span>
        <input
          v-model="form.voorvoegselAchternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Achternaam</span>
        <input
          v-model="form.achternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Organisatie</span>
        <input
          v-model="form.organisatie"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 1</span>
        <input
          ref="telEl"
          v-model="form.telefoonnummer1"
          type="tel"
          name="Telefoonnummer 1"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 2</span>
        <input
          v-model="form.telefoonnummer2"
          type="tel"
          name="Telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Omschrijving telefoonnummer 2</span>
        <input
          v-model="form.omschrijvingTelefoonnummer2"
          name="Omschrijving telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>E-mailadres</span>
        <input
          v-model="form.emailadres"
          type="email"
          name="E-mailadres"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
    </form-fieldset>
  </div>
</template>

<script lang="ts" setup>
import MedewerkerSearch from "@/features/search/MedewerkerSearch.vue";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { ref } from "vue";
import { watch } from "vue";
import {
  FormFieldsetLegend,
  FormFieldset,
} from "@utrecht/component-library-vue";
import AfdelingSearch from "./AfdelingSearch.vue";
import GroepSearch from "./GroepSearch.vue";
const props = defineProps<{
  modelValue: ContactmomentContactVerzoek;
}>();
const form = ref<Partial<ContactmomentContactVerzoek>>({});
watch(
  () => props.modelValue,
  (v) => {
    form.value = v;
  },
  { immediate: true },
);

const setActive = () => {
  form.value.isActive = true;
};

const telEl = ref<HTMLInputElement>();

watch(
  [
    telEl,
    () =>
      !!form.value.telefoonnummer1 ||
      !!form.value.telefoonnummer2 ||
      !!form.value.emailadres,
  ],
  ([el, hasContact]) => {
    if (!el) return;
    el.setCustomValidity(
      hasContact ? "" : "Vul minimaal een telefoonnummer of een e-mailadres in",
    );
  },
);

watch(
  () => form.value.afdeling,
  () => {
    form.value.groep = undefined;
  },
);
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

textarea {
  resize: none;
}

fieldset {
  display: grid;
  gap: var(--spacing-extrasmall);
}
</style>
