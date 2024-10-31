<template>
  <contactmomenten-overzicht
    v-if="contactmomenten?.page"
    :contactmomenten="contactmomenten.page"
  />
</template>
<script setup lang="ts">
import type { PaginatedResult } from "@/services";
import type { ContactmomentViewModel } from "@/services/openklant2";
import { useAsync } from "@/services/use-async";
import { watchEffect } from "vue";
import ContactmomentenOverzicht from "./ContactmomentenOverzicht.vue";
import { fetchContactmomentenByObjectUrl } from "./service";

const props = defineProps<{
  objectUrl: string;
  gebruikKlantInteracties: boolean;
}>();

const emit = defineEmits<{
  load: [data: PaginatedResult<ContactmomentViewModel>];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const {
  data: contactmomenten,
  loading,
  error,
} = useAsync(() => {
  if (props.objectUrl)
    return fetchContactmomentenByObjectUrl(
      props.objectUrl,
      props.gebruikKlantInteracties,
    );
});

watchEffect(() => contactmomenten.value && emit("load", contactmomenten.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
