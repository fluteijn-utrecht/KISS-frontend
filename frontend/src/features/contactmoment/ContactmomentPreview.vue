<template>
  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <dt>Aangemaakt door</dt>
      <dd>{{ fullName(data.medewerkerIdentificatie) }}</dd>
      <slot
        name="object"
        :object="object"
        v-for="(object, k) in data.objectcontactmomenten"
        :key="k"
      >
      </slot>
    </template>
  </service-data-wrapper>
  <service-data-wrapper :data="details">
    <template #success="{ data }">
      <template v-if="data">
        <dt>Vraag</dt>
        <dd>{{ data.vraag }}</dd>
        <dt>Specifieke vraag</dt>
        <dd>{{ data.specifiekeVraag }}</dd>
      </template>
    </template>
  </service-data-wrapper>
  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <template v-if="data">
        <dt>Toelichting</dt>
        <dd>{{ data.tekst }}</dd>
      </template>
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentByUrl, useContactmomentDetails } from "./service";
import { fullName } from "@/helpers/string";

const props = defineProps<{ url: string }>();
const details = useContactmomentDetails(() => props.url);
const cm = useContactmomentByUrl(() => props.url);
</script>
