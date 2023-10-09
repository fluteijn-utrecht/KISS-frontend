<template>
  <service-data-wrapper :data="details">
    <template #success="{ data }">
      <slot name="details" :details="data" />
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentDetails } from "./service";

const props = defineProps<{ url: string }>();
const details = useContactmomentDetails(() => props.url);
</script>
