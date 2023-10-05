<template>
  <service-data-wrapper :data="details">
    <template #success="{ data }">
      <template v-if="data">
        <slot name="details" :details="data" />
      </template>
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentDetails } from "./service";

const props = defineProps<{ url: string }>();
const details = useContactmomentDetails(() => props.url);
</script>
