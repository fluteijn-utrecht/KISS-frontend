<template>
  <teleport :to="tablist.el.value" v-if="tablist?.el.value">
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
import { inject, computed, watchEffect } from "vue";
import { tablistInjectionKey } from "./injection";

const props = defineProps<{ disabled?: boolean; label: string }>();
const tablist = inject(tablistInjectionKey);
const tabId = nanoid();
const panelId = tabId + "_panel";
const isActive = computed(() => tablist?.isActive(props.label) || false);
const is = computed(() => (!props.disabled && !isActive.value ? "a" : "span"));
const href = computed(() => (is.value === "a" ? "#" + panelId : undefined));
const activate = () => {
  if (!props.disabled) {
    tablist?.setActive(props.label);
  }
};

watchEffect(() => {
  if (props.disabled) {
    tablist?.unregister(props.label);
  } else {
    tablist?.register(props.label);
  }
});
</script>

<style lang="scss" scoped>
[role="tab"] {
  text-decoration: none;
  color: inherit;
  padding-inline: var(--spacing-large);
  padding-block: var(--spacing-default);

  &[inert] {
    color: var(--color-grey);
  }
}

[role="tab"][aria-selected="true"],
[role="tabpanel"] {
  background-color: var(--tab-bg, var(--color-secondary));
}

[role="tabpanel"] {
  padding: var(--spacing-large);

  :deep(.details-block) {
    padding: 0;
  }

  :deep(tbody > tr) {
    background: var(--color-white);
  }
}
</style>
