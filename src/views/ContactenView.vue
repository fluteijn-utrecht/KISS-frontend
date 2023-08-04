<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contacten</utrecht-heading>
    <contact-zoeker />
  </div>
</template>

<script setup lang="ts">
import ContactZoeker from "@/features/klant/contact/ContactZoeker.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
