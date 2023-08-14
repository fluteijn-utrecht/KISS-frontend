<template>
  <service-data-wrapper :data="contactmoment">
    <template #success="{ data }">
      <dl>
        <dt>Vraag</dt>
        <dd>{{ data.vraag }}</dd>
      </dl>
      <dl>
        <dt>Specifieke vraag</dt>
        <dd>{{ data.specifiekevraag }}</dd>
      </dl>
      <dl>
        <dt>Toelichting</dt>
        <dd>{{ data.tekst }}</dd>
      </dl>
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentDetails } from "./service";
const props = defineProps<{ url: string }>();
const contactmoment = useContactmomentDetails(() => props.url);
</script>
