<template>
  <label
    class="utrecht-form-label"
    v-if="afdelingVragenSets && afdelingVragenSets.length > 0"
  >
    <span>Onderwerp</span>
    <select
      class="utrecht-select utrecht-select--html-select"
      name="VragenSets"
      v-model="vragenSetId"
      @change="setOnderwerp"
    >
      <option value="" selected>Geen</option>
      <option
        v-for="item in afdelingVragenSets"
        :key="item.id"
        :value="item.id"
      >
        {{ item.titel }}
      </option>
    </select>
  </label>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type { ContactVerzoekVragenSet } from "./types";

const props = defineProps<{
  afdelingId?: string;
  vragenSets: ContactVerzoekVragenSet[];
}>();
// const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
// const modelValue = computed({
//   get: () => props.modelValue,
//   set: (val) => emit("update:modelValue", val),
// });

const vragenSetId = ref<string | undefined>();

const afdelingVragenSets = computed(() => {
  const selectedAfdelingId = props.afdelingId;

  return props.vragenSets.filter(
    (s) => s.afdelingId == selectedAfdelingId && selectedAfdelingId,
  );
});

const setOnderwerp = () => {
  //doe iets
};
</script>
