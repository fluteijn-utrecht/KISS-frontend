<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      :placeholder="placeholder"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :list-items="result"
      :exact-match="true"
      :required="required"
      :disabled="isDisabled"
      ref="searchCombo"
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
import {
  searchMedewerkers,
  useFilteredSearch,
} from "@/features/search/service";
import type { SearchResult } from "@/features/search/types";
import SearchCombobox from "@/components/SearchCombobox.vue";
import {
  mapServiceData,
  ServiceResult,
  type ServiceData,
  fetchLoggedIn,
} from "@/services";
import type { PropType } from "vue";

type DatalistItem = {
  value: string;
  description: string;
};

const props = defineProps({
  modelValue: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
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

// function mapDatalistItem(
//   x: SearchResult,
//   // eslint-disable-next-line @typescript-eslint/no-explicit-any
// ): DatalistItem & { obj: Record<string, any> } {
//   const functie = x?.jsonObject?.functie || x?.jsonObject?.function;
//   const department =
//     x?.jsonObject?.afdelingen?.[0]?.afdelingnaam || x?.jsonObject?.department;

//   const werk = [functie, department].filter(Boolean).join(" bij ");
//   return {
//     obj: x.jsonObject,
//     value: x.title,
//     description: werk,
//   };
// }

const emit = defineEmits(["update:modelValue"]);

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function updateModelValue(v: string) {
  searchText.value = v;
  if (result?.value?.success) {
    const match = result.value.data.find(
      (x: { value: string }) => x?.value === v,
    );

    emit(
      "update:modelValue",
      match && {
        //wat moet hier en global serach fixen..
        ...match,
        title: match.value,
        achternaam: match.value,
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

// const filteredSearchParams = computed(() => {
//   return {
//     filterField: props.filterField,
//     filterValue: props.filterValue,
//     search: debouncedSearchText.value,
//   };
// });

// const result = useFilteredSearch(filteredSearchParams);
//  const datalistItems = mapServiceData(result, (paginated) =>
//   paginated.page.map(mapDatalistItem),
//  );

// watch([() => props.filterField, () => props.filterValue], () => {
//   result.refresh();
// });

const result = ref<ServiceData<DatalistItem[]>>(ServiceResult.init());

// const datalistItems = computed(() => {
//   if (result.value?.success) {
//     return mapServiceData(result.value, (paginated) =>
//       paginated.map((xx) => {
//         xx;
//       }),
//     );
//   }
//   return undefined;
// });

watch(
  [
    () => props.filterField,
    () => props.filterValue,
    () => debouncedSearchText.value,
  ],
  () => {
    // d.value = ServiceResult.loading()(
    //   props.filterField,
    //   props.filterValue,
    //   debouncedSearchText,
    // );
    // " props.filterField, props.filterValue, debouncedSearchText    ";

    result.value = ServiceResult.fromPromise<DatalistItem[]>(
      searchMedewerkers({
        search: debouncedSearchText.value,
        filterField: props.filterField,
        filterValue: props.filterValue,
      }),
    );
  },
);
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
