<template>
  <teleport :to="tablist.el.value" v-if="tablist.el.value">
    <component
      role="tab"
      :aria-selected="isActive ? 'true' : 'false'"
      :aria-controls="panelId"
      :inert="disabled"
      :href="href"
      :id="tabId"
      :is="is"
      @click.prevent="activate"
    >
      <slot name="tab" :label="label" :is-active="isActive">
        {{ label }}
      </slot>
    </component>
  </teleport>
  <section
    :key="panelId"
    :id="panelId"
    role="tabpanel"
    :aria-labelledby="tabId"
    v-show="isActive"
    v-bind="$attrs"
  >
    <slot :is-active="isActive"></slot>
  </section>
</template>

<script setup lang="ts">
import { nanoid } from "nanoid";
import { inject, computed } from "vue";
import type { Tablist } from "./TabsNew.vue";

const props = defineProps<{ disabled?: boolean; label?: string }>();
const tablist = inject<Tablist>("tablist");
if (!tablist) {
  throw new Error("use this component in a tablist");
}
const tabId = nanoid();
const panelId = tabId + "_panel";
const isActive = computed(() => tablist.isActive(tabId));
const is = computed(() => (!props.disabled && !isActive.value ? "a" : "span"));
const href = computed(() => (is.value === "a" ? "#" + panelId : undefined));
const activate = () => {
  if (!props.disabled) {
    tablist.setActive(tabId);
  }
};
</script>

<style lang="scss" scoped>
[role="tab"] {
  text-decoration: none;
  color: inherit;

  &[inert] {
    color: var(--color-grey);
  }
}
</style>
