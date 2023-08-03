<template>
  <tabs-list-item :label="label" :disabled="disabled" class="data-tabpanel">
    <template #tab="{ isActive }">
      <div class="data-tab" :data-active="isActive">
        <span :class="data.error ? 'alert icon-after' : ''">{{ label }}</span>
        <simple-spinner class="small-spinner" v-if="data.loading" />
      </div>
    </template>
    <template #default="{ isActive }">
      <div class="data-tabpanel" :data-active="isActive">
        <service-data-wrapper :data="data">
          <template #success="{ data }">
            <slot name="success" :data="data"></slot>
          </template>
        </service-data-wrapper>
      </div>
    </template>
  </tabs-list-item>
</template>

<script setup lang="ts" generic="T">
import type { ServiceData } from "@/services";
import { computed } from "vue";
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import TabsListItem from "./TabsListItem.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

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
  position: relative;

  > span {
    display: flex;
    gap: 1ch;
  }
}

.small-spinner {
  --spinner-size: 1em;

  position: absolute;
  inset: 0;
  translate: 0 50%;
}
</style>
