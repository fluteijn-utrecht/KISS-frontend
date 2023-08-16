<template>
  <simple-spinner v-if="zaak.loading" />

  <application-message
    v-if="zaak.error"
    message="Er ging iets mis bij het ophalen van de gegevens. Probeer het later nog eens."
    messageType="error"
  />

  <template v-if="zaak.success && zaak.data">
    <dt>Zaaknummer</dt>
    <dd>{{ zaak.data.identificatie }}</dd>
    <dt>Zaaktype</dt>
    <dd>{{ zaak.data.zaaktypeLabel }}</dd>
    <dt>Zaakstatus</dt>
    <dd>{{ zaak.data.status }}</dd>
  </template>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useZakenPreviewByUrl } from "../service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const props = defineProps<{
  zaakurl: string;
}>();

const zaak = useZakenPreviewByUrl(computed(() => props.zaakurl));
</script>
