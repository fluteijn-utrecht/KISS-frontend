<template>
  <div>
    <search-combobox
      v-bind="$attrs"
      :id="id"
      :required="required"
      placeholder="Zoek een afdeling"
      v-model="searchText"
      @update:model-value="updateModelValue"
      :list-items="datalistItems"
      :exact-match="true"
    />
  </div>
</template>

<script lang="ts" setup generic="T">
import { debouncedRef } from "@vueuse/core";
import { computed, ref, watch } from "vue";
import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";
import { type PaginatedResult, type ServiceData } from "@/services";

defineOptions({
  inheritAttrs: false,
});

const props = defineProps<{
  modelValue: T | undefined;
  id?: string;
  required?: boolean;
  mapValue: (x: T) => string;
  mapDescription: (x: T) => string;
  getData: (x: () => string | undefined) => ServiceData<PaginatedResult<T>>;
}>();

function mapDatalistItem(x: T): DatalistItem & { obj: T } {
  return {
    obj: x,
    value: props.mapValue(x),
    description: props.mapDescription(x),
  };
}

const emit = defineEmits<{
  "update:modelValue": [T | undefined];
}>();

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function updateModelValue(v: string) {
  searchText.value = v;
  if (data.success) {
    const match = data.data.page.find((x) => props.mapValue(x) === v);
    emit("update:modelValue", match);
  }
}

watch(
  () => props.modelValue,
  (v) => {
    searchText.value = v === undefined ? "" : props.mapValue(v) || "";
  },
  { immediate: true },
);

// INTENTIONAL
// eslint-disable-next-line vue/no-setup-props-destructure
const data = props.getData(() => debouncedSearchText.value);

const datalistItems = computed(() => ({
  ...data,
  data: data.success ? data.data.page.map(mapDatalistItem) : [],
}));
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
