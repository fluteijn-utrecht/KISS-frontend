<template>
  <nav role="tablist" ref="el" />
  <slot></slot>
</template>

<script lang="ts">
import type { InjectionKey, Ref } from "vue";
export const tablistInjectionKey = Symbol() as InjectionKey<{
  el: Ref<HTMLElement | undefined>;
  setActive: (name: string) => void;
  isActive: (name: string) => boolean;
  register: (name: string) => void;
}>;
</script>

<script setup lang="ts">
import { watch } from "vue";
import { computed, reactive } from "vue";
import { provide, ref } from "vue";

const props = defineProps<{ modelValue: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v: string): void }>();

const current = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v),
});

const tabIds = reactive(new Set<string>());

const setActive = (name: string) => (current.value = name);
const isActive = (name: string) => current.value === name;
const register = (name: string) => {
  tabIds.add(name);
};

const el = ref<HTMLElement>();

provide(tablistInjectionKey, {
  el,
  setActive,
  isActive,
  register,
});

watch([current, tabIds], ([c, ids]) => {
  if (!c && ids.size) {
    setActive([...ids.values()][0] || "");
  }
});
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
