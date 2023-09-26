<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contactverzoeken</utrecht-heading>
    <contactverzoeken-zoeker />
  </div>
</template>

<script setup lang="ts">
import ContactverzoekenZoeker from "@/features/klant/contactverzoek/ContactverzoekenZoeker.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
