<template>
  <tabs-tab :label="label" :disabled="disabled" class="data-tabpanel">
    <template #tab="{ isActive }">
      <div class="data-tab" :data-active="isActive">
        <span :class="data.error ? 'alert icon-after' : ''">{{ label }}</span>
        <simple-spinner class="small-spinner" v-if="data.loading" />
      </div>
    </template>
    <template #default="{ isActive }">
      <div class="data-tabpanel" :data-active="isActive">
        <data-wrapper :data="data">
          <template #success="{ data }">
            <slot name="success" :data="data"></slot>
          </template>
        </data-wrapper>
      </div>
    </template>
  </tabs-tab>
</template>

<script setup lang="ts" generic="T">
import type { ServiceData } from "@/services";
import { computed } from "vue";
import DataWrapper from "./DataWrapper.vue";
import TabsTab from "./TabsTab.vue";
import SimpleSpinner from "./SimpleSpinner.vue";

const props = defineProps<{
  label: string;
  data: ServiceData<T>;
  disabled: (data: T) => boolean;
}>();

const disabled = computed(
  () => props.data.success && props.disabled(props.data.data)
);
</script>

<style lang="scss" scoped>
.data-tab {
  padding-inline: var(--spacing-large);
  padding-block: var(--spacing-default);
  position: relative;

  > span {
    display: flex;
    gap: 1ch;
  }
}

.data-tabpanel {
  flex: 1;
  padding: var(--spacing-large);

  tbody tr {
    background: var(--color-white);
  }
}

[data-active="true"] {
  background-color: var(--tab-bg, var(--color-secondary));
}

.small-spinner {
  --spinner-size: 1em;

  position: absolute;
  inset: 0;
  translate: 0 50%;
}
</style>
