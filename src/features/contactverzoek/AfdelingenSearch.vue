<template>
  <search-combobox v-model="search" :list-items="dataItems" v-bind="$attrs" />
</template>

<script setup lang="ts">
// TODO: hoort eigenlijk niet bij contactverzoek feature maar bij contactmoment
// Maar: waar moet de service (useAfdelingen) dan naartoe?
// Dit is een service die vanuit meerdere features aangeroepen zou worden
// Overleggen

import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";
import { useAfdelingen } from ".";
import { mapServiceData } from "@/services";
import { computed } from "vue";

const props = defineProps<{
  modelValue: string | undefined;
}>();

const emit = defineEmits<{
  "update:modelValue": [val: string | undefined];
}>();

const search = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v),
});

const afdelingen = useAfdelingen(() => props.modelValue);
const dataItems = mapServiceData(afdelingen, (paginated) =>
  paginated.page.map<DatalistItem>(({ identificatie, naam }) => ({
    description: identificatie,
    value: naam,
  })),
);
</script>
