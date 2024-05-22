<template>
  <service-data-search
    :get-data="useGroepen"
    :map-value="(x: Groep) => x.naam"
    :map-description="(x: Groep) => x.identificatie"
    :model-value="modelValue"
    @update:modelValue="$emit('update:modelValue', $event)"
    v-bind="$attrs"
  />
</template>

<script setup lang="ts">
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { useGroepen, type Groep } from "@/composables/groepen";
defineProps<{ modelValue: Groep | undefined }>();
defineEmits<{ "update:modelValue": [Groep | undefined] }>();
</script>
