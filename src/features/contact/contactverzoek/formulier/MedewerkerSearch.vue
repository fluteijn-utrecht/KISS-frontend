<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      :placeholder="placeholder"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :result="result"
      :list-items="datalistItems"
      :exact-match="true"
      :required="required"
      :disabled="isDisabled"
    />
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { debouncedRef } from "@vueuse/core";
import { computed, ref, watch } from "vue";
import { useFilteredSearch } from "@/features/search/service";
import type { SearchResult } from "@/features/search/types";
import SearchCombobox from "@/components/SearchCombobox.vue";
import { mapServiceData } from "@/services";
import type { PropType } from "vue";

type DatalistItem = {
  value: string;
  description: string;
};

const props = defineProps({
  modelValue: {
    type: Object as PropType<Record<string, any> | undefined>,
  },
  id: {
    type: String,
    default: undefined,
  },
  filterField: {
    type: String,
    default: undefined,
  },
  filterValue: {
    type: String,
    default: undefined,
  },
  required: {
    type: Boolean,
    default: false,
  },
  isDisabled: {
    type: Boolean,
    default: false,
  },
  placeholder: {
    type: String,
    default: "Zoek een medewerker",
  },
});

function mapDatalistItem(
  x: SearchResult,
): DatalistItem & { obj: Record<string, any> } {
  const functie = x?.jsonObject?.functie || x?.jsonObject?.function;
  const department =
    x?.jsonObject?.afdelingen?.[0]?.afdelingnaam || x?.jsonObject?.department;

  const werk = [functie, department].filter(Boolean).join(" bij ");
  return {
    obj: x.jsonObject,
    value: x.title,
    description: werk,
  };
}

const emit = defineEmits(["update:modelValue"]);

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function updateModelValue(v: any) {
  searchText.value = v;
  if (result.success) {
    const match = result.data.page.find((x) => x?.title === v);
    emit(
      "update:modelValue",
      match && {
        ...match.jsonObject,
        title: match.title,
      },
    );
  }
}

watch(
  () => props.modelValue,
  (v) => {
    searchText.value = v?.title;
  },
  { immediate: true },
);

const filteredSearchParams = computed(() => {
  return {
    filterField: props.filterField,
    filterValue: props.filterValue,
    search: debouncedSearchText.value,
  };
});

const result = useFilteredSearch(filteredSearchParams);
const datalistItems = mapServiceData(result, (paginated) =>
  paginated.page.map(mapDatalistItem),
);

watch([() => props.filterField, () => props.filterValue], () => {
  result.refresh();
});
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
