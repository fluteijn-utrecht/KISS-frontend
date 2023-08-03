<template>
  <section class="klant-panel">
    <utrecht-heading :level="1">Bedrijven</utrecht-heading>
    <bedrijf-zoeker />
  </section>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { BedrijfZoeker } from "@/features/klant";
</script>
