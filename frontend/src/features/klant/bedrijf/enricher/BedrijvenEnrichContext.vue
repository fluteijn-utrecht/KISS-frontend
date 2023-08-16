<template>
  <dialog ref="dialog">
    <simple-spinner />
  </dialog>
  <bedrijf-enrich-context
    v-for="(record, idx) in records"
    :key="idx"
    :record="record"
  >
    <template #default="{ enriched }">
      <slot
        :enriched="{ ...enriched, create: wrapCreate(enriched.create) }"
      ></slot>
    </template>
  </bedrijf-enrich-context>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BedrijfEnrichContext from "./BedrijfEnrichContext.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { Bedrijf } from "../types";
import type { Klant } from "@/features/klant";

defineProps<{ records: Array<Bedrijf | Klant> }>();

const dialog = ref<HTMLDialogElement>();
const wrapCreate = (create: () => Promise<void>) => () => {
  if (dialog.value?.open) return Promise.resolve();
  dialog.value?.showModal();
  return create().finally(() => {
    dialog.value?.close();
  });
};
</script>

<style lang="scss" scoped>
dialog[open] {
  width: 100%;
  height: 100%;
  display: flex;
  place-content: center;
  place-items: center;
}
</style>
