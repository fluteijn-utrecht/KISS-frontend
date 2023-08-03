<template>
  <div>
    <nav role="tablist" ref="el" />
    <slot> </slot>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted } from "vue";
import { provide, ref } from "vue";

const current = ref("");

const setActive = (name: string) => (current.value = name);
const isActive = (name: string) => current.value === name;

const el = ref<HTMLElement>();

function getFirstId() {
  if (!el.value) return undefined;
  for (const iterator of el.value.children) {
    if (iterator.id) return iterator.id;
  }
  return undefined;
}

onMounted(() => {
  nextTick(() => {
    if (!current.value) {
      const firstId = getFirstId();
      if (firstId) {
        setActive(firstId);
      }
    }
  });
});

const tablist = {
  el,
  setActive,
  isActive,
} as const;

export type Tablist = typeof tablist;

provide("tablist", tablist);
</script>

<style lang="scss" scoped>
[role="tablist"] {
  display: flex;
  font-size: var(--tab-size, 1rem);
  font-weight: bold;
  color: var(--tab-color, var(--utrecht-heading-color));
  gap: var(--tab-gap, 0.5em);
}
</style>
