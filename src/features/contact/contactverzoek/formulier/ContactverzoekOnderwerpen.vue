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
import { computed, ref, watch } from "vue";
import type { ContactVerzoekVragenSet } from "./types";
import { watchEffect } from "vue";

const props = defineProps<{
  afdelingId?: string; //de afdeling waarvan vragensets getoond mogen worden in de keuzelijst
  vragenSets: ContactVerzoekVragenSet[]; //alle vragensets
  modelValue?: ContactVerzoekVragenSet; //de (voor)geselecteerde vragenset
  prefill: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v?: ContactVerzoekVragenSet): void;
  (e: "change"): void;
}>();

//subset van vragensets horende bij de geselecteerde afdeling
const afdelingVragenSets = computed(() => {
  const selectedAfdelingId = props.afdelingId;
  return props.vragenSets.filter(
    (s) => s.afdelingId == selectedAfdelingId && selectedAfdelingId,
  );
});

//tbv modelbinding van de (voor)gekozen vragenset
const vragenSetId = ref<number | undefined>();

watchEffect(() => {
  //eerder gekozen waarde voorselecteren
  vragenSetId.value = props.modelValue?.id;
});

watch(
  afdelingVragenSets,
  (v) => {
    if (v && v.length > 0 && !vragenSetId.value && props.prefill) {
      vragenSetId.value = v[0].id;
      emit("update:modelValue", v[0]);
    }
  },
  { immediate: true },
);

watch(
  () => props.afdelingId,
  (v) => {
    if (!v) {
      vragenSetId.value = undefined;
      emit("update:modelValue", undefined);
    }
  },
  { immediate: true },
);

const setOnderwerp = () => {
  //wanneer een item uit de lijst gekozen is, de bijbehorende vragenset opzoeken en emitten
  const vragenset = props.vragenSets.find((x) => {
    return x.id === vragenSetId?.value;
  });
  emit("update:modelValue", vragenset);
  emit("change");
};
</script>
