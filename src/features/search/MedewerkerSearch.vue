<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      placeholder="Zoek een medewerker"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :list-items="datalistItems"
      :exact-match="true"
      :required="true"
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
import { useGlobalSearch, useSources } from "./service";
import type { SearchResult } from "./types";
import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";
import { mapServiceData } from "@/services";
import type { PropType } from "vue";

const props = defineProps({
  modelValue: {
    type: Object as PropType<Record<string, any> | undefined>,
  },
  id: {
    type: String,
    default: undefined,
  },
  required: {
    type: Boolean,
    default: false,
  },
});

function mapDatalistItem(
  x: SearchResult,
): DatalistItem & { obj: Record<string, any> } {
  const { department, function: functie } = x?.jsonObject ?? {};

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

const sources = useSources();

const searchParams = computed(() => {
  if (sources.success) {
    const smoelen = sources.data.find((x) => x.name === "Smoelenboek");
    if (smoelen) {
      return {
        filters: [smoelen],
        search: debouncedSearchText.value,
      };
    }
  }
  return {
    filters: [],
  };
});

const result = useGlobalSearch(searchParams);
const datalistItems = mapServiceData(result, (paginated) =>
  paginated.page.map(mapDatalistItem),
);
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
