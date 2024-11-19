<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contactverzoeken</utrecht-heading>
    <contactverzoeken-zoeker>
      <template #object="{ object }">
        <zaak-preview v-if="object.object" :zaakurl="object.object" />
      </template>
    </contactverzoeken-zoeker>
  </div>
</template>

<script setup lang="ts">
import ContactverzoekenZoeker from "@/features/contact/contactverzoek/overzicht/ContactverzoekenZoeker.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
