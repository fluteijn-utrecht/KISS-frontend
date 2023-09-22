<template>
  <table class="overview">
    <slot name="caption" />

    <contactverzoeken-overzicht :contactverzoeken="records">
      <template #contactmoment="{ url }">
        <contactmoment-preview :url="url">
          <template #object="{ object }">
            <zaak-preview :zaakurl="object.object" />
          </template>
        </contactmoment-preview>
      </template>
    </contactverzoeken-overzicht>
  </table>
</template>

<script lang="ts" setup>
import type { Klant } from "../types";

//todo: GEBRUIKT EEN COMPONETN UIT EEN ANDER FEATURE. MAG NIET. GENERIEK MAKEN OF DUPLICEREN
import ContactverzoekenOverzicht from "@/features/contactverzoek/ContactverzoekenOverzicht.vue";
defineProps<{ records: any[] }>();
</script>

<style lang="scss" scoped>
.overview {
  inline-size: 30rem;
  min-inline-size: max-content;
}
</style>
