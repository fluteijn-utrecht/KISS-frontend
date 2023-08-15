<template>
  <service-data-wrapper :data="contactmoment">
    <template #success="{ data }">
      <dt>Aangemaakt door</dt>
      <dd>{{ fullName(data.medewerkerIdentificatie) }}</dd>
      <template v-if="data.zaken">
        <slot v-for="(z, k) in data.zaken" :key="k" name="zaak" :url="z">
        </slot>
      </template>
      <dt>Vraag</dt>
      <dd>{{ data.vraag }}</dd>
      <dt>Specifieke vraag</dt>
      <dd>{{ data.specifiekevraag }}</dd>
      <dt>Toelichting</dt>
      <dd>{{ data.tekst }}</dd>
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentDetails } from "./service";
import { fullName } from "@/helpers/string";

const props = defineProps<{ url: string }>();
const contactmoment = useContactmomentDetails(() => props.url);
</script>
