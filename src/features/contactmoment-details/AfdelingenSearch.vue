<template>
  <service-data-search
    :get-data="useAfdelingen"
    :map-value="(x) => x.naam"
    :map-description="(x) => x.identificatie"
    :model-value="modelValue"
    @update:modelValue="$emit('update:modelValue', $event)"
    v-bind="$attrs"
  />
</template>

<script setup lang="ts">
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { useAfdelingen, type Afdeling } from "@/features/shared/afdelingen";
defineProps<{ modelValue: Afdeling | undefined }>();
defineEmits<{ "update:modelValue": [Afdeling | undefined] }>();
</script>
