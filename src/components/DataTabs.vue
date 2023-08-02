<template>
  <tabs-component v-model="currentTab">
    <template #tab="{ tabName }">
      <span :ref="refs[tabName]">{{ tabName }}</span>
      <simple-spinner
        class="small-spinner"
        v-if="entries.find(([key]) => key === tabName)?.[1]?.[0]?.loading"
      />
    </template>

    <template v-for="[key, [data]] in entries" :key="key" #[key.toString()]>
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
type Entries<T> = {
  [K in keyof T]: [K, T[K]];
}[keyof T][];
export type TabStateValue<V> = [ServiceData<V>, (v: V) => boolean];

export type TabState<K extends string = string, V = any> = {
  [k in K]: TabStateValue<V>;
};

export function tabState<K extends string>(tabState: TabState<K>) {
  return tabState;
}

export function tabStateValue<T>(
  d: ServiceData<T>,
  m: (v: T) => boolean
): TabStateValue<T> {
  return [d, m];
}
</script>

<script setup lang="ts" generic="T extends TabState">
import type { ServiceData } from "@/services";
import TabsComponent from "./TabsComponent.vue";
import { ref, watchEffect } from "vue";
import { computed } from "vue";
import SimpleSpinner from "./SimpleSpinner.vue";
import ApplicationMessage from "./ApplicationMessage.vue";

type GetDataType<K extends keyof T> = T[K] extends TabStateValue<infer U>
  ? U
  : unknown;

type Mapped = {
  [K in keyof T]: (props: { data: GetDataType<K> }) => any;
};

defineSlots<Mapped>();

const props = defineProps<{
  state: T;
}>();

const currentTab = ref("");

const refs = computed(() =>
  Object.fromEntries(Object.keys(props.state).map((key) => [key, ref()]))
);

const entries = computed(() => Object.entries(props.state) as Entries<T>);

watchEffect(() => {
  for (const key in props.state) {
    const el = refs.value[key];
    if (el.value?.parentElement) {
      const [state, func] = props.state[key];
      el.value.parentElement.inert = state.success && !func(state.data);
    }
  }
});
</script>

<style lang="scss" scoped>
:deep([role="tab"]) {
  position: relative;

  &[inert] {
    color: var(--color-secondary);
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
}
</style>
