<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      placeholder="Zoek een afdeling"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :list-items="datalistItems"
      :exact-match="true"
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
import { ref, watch } from "vue";
import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";
import { mapServiceData } from "@/services";
import { useAfdelingen } from ".";

type Afdeling = {
  id: string;
  identificatie: string;
  naam: string;
};

const props = defineProps<{
  modelValue?: Afdeling;
  id?: string;
  required?: boolean;
}>();

function mapDatalistItem(
  x: Afdeling,
): DatalistItem & { obj: Record<string, any> } {
  return {
    obj: x,
    value: x.naam,
    description: x.identificatie,
  };
}

const emit = defineEmits(["update:modelValue"]);

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function updateModelValue(v: any) {
  searchText.value = v;
  if (result.success) {
    const match = result.data.page.find((x) => x?.naam === v);
    emit("update:modelValue", match);
  }
}

watch(
  () => props.modelValue,
  (v) => {
    searchText.value = v?.naam || "";
  },
  { immediate: true },
);

const result = useAfdelingen(() => debouncedSearchText.value);
const datalistItems = mapServiceData(result, (paginated) =>
  paginated.page.map(mapDatalistItem),
);
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
