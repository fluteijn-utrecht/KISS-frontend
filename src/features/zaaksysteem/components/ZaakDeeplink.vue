<template>
  <a v-if="deeplink" :href="deeplink" target="_blank" rel="noopener noreferrer"
    >Open in zaaksysteem</a
  >
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ZaakDetails } from "../types";
import { useZaaksysteemDeeplinkConfig } from "../service";
const props = defineProps<{
  zaak: ZaakDetails;
}>();

const config = useZaaksysteemDeeplinkConfig(() => props.zaak.zaaksysteemId);

const deeplink = computed(() => {
  if (!config.success || !config.data) return null;
  const property = (props.zaak as Record<string, unknown>)[
    config.data.idProperty
  ];
  if (!property) return null;
  return config.data.baseUrl + property;
});
</script>
