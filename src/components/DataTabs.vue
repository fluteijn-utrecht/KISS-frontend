<template>
  <tabs-component v-model="currentTab">
    <template #tab="{ tabName }">
      <span
        :ref="refs[tabName]"
        :class="state[tabName][0].error ? 'alert icon-after' : ''"
        >{{ tabName }}</span
      >
      <simple-spinner class="small-spinner" v-if="state[tabName][0].loading" />
    </template>

    <template v-for="[key, [data]] in entries" :key="key" #[key]>
      <simple-spinner v-if="data.loading" />
      <application-message
        v-if="data.error"
        messageType="error"
        message="Er is een fout opgetreden"
      />
      <slot v-if="data.success" :name="key" :data="data.data" />
    </template>
  </tabs-component>
</template>

<script lang="ts">
export type TabState<V> = [ServiceData<V>, (v: V) => boolean];

type TabDictionary = Record<string, TabState<any>>;

export function tabState<T>(
  d: ServiceData<T>,
  m: (v: T) => boolean
): TabState<T> {
  return [d, m];
}
</script>

<script setup lang="ts" generic="T extends TabDictionary">
import type { ServiceData } from "@/services";
import TabsComponent from "./TabsComponent.vue";
import { ref, watchEffect } from "vue";
import { computed } from "vue";
import SimpleSpinner from "./SimpleSpinner.vue";
import ApplicationMessage from "./ApplicationMessage.vue";

type GetServiceDataType<K extends keyof T> = T[K] extends TabState<infer U>
  ? U
  : unknown;

type DataTabsSlots = {
  [K in keyof T]: (props: { data: GetServiceDataType<K> }) => any;
};

const slots = defineSlots<DataTabsSlots>();

const props = defineProps<{
  state: T;
  modelValue: keyof T | undefined;
}>();

const emit = defineEmits(["update:modelValue"]);

const currentTab = computed({
  get: () => props.modelValue as string,
  set: (val) => emit("update:modelValue", val),
});

const refs = computed(() =>
  Object.fromEntries(Object.keys(props.state).map((key) => [key, ref()]))
);

const state = computed(() => props.state);

const entries = computed(() =>
  Object.keys(slots)
    .filter((x) => x in state.value)
    .map((x) => [x, state.value[x]] as const)
);

watchEffect(() => {
  for (const key in props.state) {
    const el = refs.value[key];
    if (el.value instanceof HTMLElement && el.value.parentElement) {
      const [state, func] = props.state[key];
      el.value.parentElement.inert = state.success && !func(state.data);
    }
  }
});
</script>

<style lang="scss" scoped>
:deep([role="tab"]) {
  position: relative;

  > span {
    display: flex;
    gap: 1ch;
  }

  &[inert] {
    color: var(--color-grey);
  }
}

.small-spinner {
  --spinner-size: 1em;

  position: absolute;
  inset: 0;
  translate: 0 50%;
}

:deep([role="tabpanel"]) {
  padding: var(--spacing-large);

  tbody tr {
    background: var(--color-white);
  }
}
</style>
