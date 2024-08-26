<template>
  <section :aria-labelledby="id">
    <Heading :id="id" :level="level">{{ label }}</Heading>
    <slot></slot>
  </section>
</template>

<script setup lang="ts">
import { nanoid } from "nanoid";
import { Heading } from "@utrecht/component-library-vue";

defineProps<{
  level: 1 | 2 | 3 | 4 | 5;
  label: string;
}>();
const id = nanoid();
</script>
