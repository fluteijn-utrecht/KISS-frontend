<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      placeholder="Zoek een medewerker"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :result="result"
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
  required: {
    type: Boolean,
    default: false,
  },
});

function mapDatalistItem(
  x: SearchResult,
): DatalistItem & { obj: Record<string, any> } {
  const { contact, department, function: functie, user } = x?.jsonObject ?? {};
  const { voornaam, voorvoegselAchternaam, achternaam } = contact ?? {};
  const naam = [voornaam, voorvoegselAchternaam, achternaam]
    .filter(Boolean)
    .join(" ");
  const werk = [functie, department].filter(Boolean).join(" bij ");
  const description = [naam, werk].filter(Boolean).join(": ");
  return {
    obj: x.jsonObject,
    value: user,
    description,
  };
}

const emit = defineEmits(["update:modelValue"]);

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function updateModelValue(v: any) {
  if (result.success) {
    const match = result.data.page
      .map((x) => x.jsonObject)
      .find((x) => x?.user === v);
    emit("update:modelValue", match);
  }
}

watch(
  () => props.modelValue,
  (v) => {
    searchText.value = v?.user;
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
