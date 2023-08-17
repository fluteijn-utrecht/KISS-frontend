<template>
  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <template v-if="data.medewerkerIdentificatie">
        <dt>Aangemaakt door</dt>
        <dd>{{ fullName(data.medewerkerIdentificatie) }}</dd>
      </template>
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
        <template v-if="data.vraag">
          <dt>Vraag</dt>
          <dd>{{ data.vraag }}</dd>
        </template>
        <template v-if="data.specifiekeVraag">
          <dt>Specifieke vraag</dt>
          <dd>{{ data.specifiekeVraag }}</dd>
        </template>
      </template>
    </template>
  </service-data-wrapper>
  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <template v-if="data?.tekst">
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
