<template>
  <Suspense>
    <CkEditorAsync v-model="modelValue" />
    <template #fallback>
      <SimpleSpinnerVue />
    </template>
  </Suspense>
</template>

<script setup lang="ts">
import SimpleSpinnerVue from "@/components/SimpleSpinner.vue";
import CkEditorAsync from "./CkEditorAsync.vue";
import { computed } from "vue";
const props = defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
const modelValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});
</script>
