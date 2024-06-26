<template>
  <Ckeditor :editor="x" v-model="modelValue" v-bind="$attrs" />
</template>

<script setup lang="ts">
import _Ckeditor from "@ckeditor/ckeditor5-vue";
import { computed } from "vue";
//import { CustomEditor } from "./custom-editor";

//import { ClassicEditor } from 'ckeditor5';

const props = defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
const modelValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

const x = {
  create: (...args: any) =>
    import("./custom-editor").then((r) => r.default as any),
};

//const CustomEditor = await import("./custom-editor").then((r) => r.default);
const Ckeditor = _Ckeditor.component;
</script>
