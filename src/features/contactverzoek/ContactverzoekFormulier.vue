<template>
  <SimpleSpinner v-if="afdelingen.loading" />
  <div class="container" @submit.prevent>
    <label
      class="utrecht-form-label"
      v-if="afdelingen.success && afdelingen.data.length"
    >
      Afdeling
      <select
        v-model="form.afdeling"
        @change="setActive"
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
        required
        v-model="form.medewerker"
        :defaultValue="form.medewerker"
        @update:model-value="setActive"
      />
    </label>

    <label class="utrecht-form-label">
      <span>Klantnaam</span>
      <input
        v-model="form.naam"
        type="tel"
        name="Naam"
        class="utrecht-textbox utrecht-textbox--html-input"
        @input="setActive"
      />
    </label>

    <label class="utrecht-form-label">
      <span>Telefoonnummer 1</span>
      <input
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
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import MedewerkerSearch from "@/features/search/MedewerkerSearch.vue";
import { useAfdelingen } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { ref } from "vue";
import { watch } from "vue";

const props = defineProps<{
  modelValue: ContactmomentContactVerzoek;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: ContactmomentContactVerzoek): void;
  (e: "active"): void;
}>();

const form = ref<Partial<ContactmomentContactVerzoek>>({});

watch(
  () => props.modelValue,
  (v) => {
    form.value = v;
  },
  { immediate: true, deep: true }
);

watch(
  form,
  (f) =>
    emit("update:modelValue", {
      ...props.modelValue,
      ...f,
    }),
  { deep: true }
);

const afdelingen = useAfdelingen();

const setActive = () => {
  form.value.isActive = true;
};
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
</style>
