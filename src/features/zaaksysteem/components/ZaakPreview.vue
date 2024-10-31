<template>
  <simple-spinner v-if="loading" />

  <application-message
    v-if="error"
    message="Er ging iets mis bij het ophalen van de gegevens. Probeer het later nog eens."
    messageType="error"
  />

  <template v-if="zaak">
    <dt>Zaaknummer</dt>
    <dd>{{ zaak.identificatie }}</dd>
  </template>
</template>

<script setup lang="ts">
import { fetchZakenPreviewByUrlOrId } from "../service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { useAsync } from "@/services/use-async";

const props = defineProps<{
  zaakurl: string;
}>();

const {
  data: zaak,
  error,
  loading,
} = useAsync(() => fetchZakenPreviewByUrlOrId(props.zaakurl));
</script>
