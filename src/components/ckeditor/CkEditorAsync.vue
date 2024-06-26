<template>
  <ckeditor
    :editor="ClassicEditor"
    :config="config"
    v-model="modelValue"
    v-bind="$attrs"
  />
</template>

<script setup lang="ts">
import type { EditorConfig } from "./ckeditor-exports";
import { computed } from "vue";

const props = defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
const modelValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

// we import these asynchronously so the code gets split into a seperate file.
// we don't directly import from ckeditor5 because otherwise we can't tree shake, resulting in a bigger bundle
const { ClassicEditor, Ckeditor, ...plugins } = await import(
  "./ckeditor-exports"
);

const config: EditorConfig = {
  plugins: Object.values(plugins),
  toolbar: [
    "heading",
    "|",
    "bold",
    "italic",
    "link",
    "bulletedList",
    "numberedList",
    "blockQuote",
    "undo",
    "redo",
  ],
};
</script>

<style src="ckeditor5/ckeditor5.css"></style>
