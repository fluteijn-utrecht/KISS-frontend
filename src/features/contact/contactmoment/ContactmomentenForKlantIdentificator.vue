<template>
  <contactmomenten-overzicht
    v-if="contactmomenten"
    :contactmomenten="contactmomenten"
  >
    <template v-for="(_, slotName) in $slots" #[slotName]="props">
      <slot :name="slotName" v-bind="props"></slot>
    </template>
  </contactmomenten-overzicht>
</template>
<script setup lang="ts">
import type { ContactmomentViewModel } from "@/services/openklant2";
import { useLoader } from "@/services/use-loader";
import { watchEffect } from "vue";
import ContactmomentenOverzicht from "./ContactmomentenOverzicht.vue";
import { fetchContactmomentenByKlantIdentificator } from "./fetch-contactmomenten-by-klant-identificator";
import type { Systeem } from "@/services/environment/fetch-systemen";
import type { KlantIdentificator } from "../types";

defineSlots();

const props = defineProps<{
  klantIdentificator: KlantIdentificator;
  systemen: Systeem[];
}>();

const emit = defineEmits<{
  load: [data: ContactmomentViewModel[]];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const {
  data: contactmomenten,
  loading,
  error,
} = useLoader(() => {
  if (props.klantIdentificator)
    return fetchContactmomentenByKlantIdentificator(
      props.klantIdentificator,
      props.systemen,
    );
});

watchEffect(() => contactmomenten.value && emit("load", contactmomenten.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
