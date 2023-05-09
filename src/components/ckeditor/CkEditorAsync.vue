<template>
  <ckeditor :editor="CustomEditor" v-model="modelValue" />
</template>

<script setup lang="ts">
import _Ckeditor from "@ckeditor/ckeditor5-vue";
import { computed } from "vue";

const props = defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
const modelValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

const CustomEditor = await import("./custom-editor").then((r) => r.default);
const Ckeditor = _Ckeditor.component;
</script>
