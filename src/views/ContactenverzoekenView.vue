<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contactverzoeken</utrecht-heading>
    <contactverzoeken-zoeker>
      <template #onderwerp="{ contactmomentUrl }">
        <contactmoment-details-context :url="contactmomentUrl">
          <template #details="{ details }">
            {{ details?.vraag || details?.specifiekeVraag }}
          </template>
        </contactmoment-details-context>
      </template>
      <template #contactmoment="{ url }">
        <contactmoment-preview :url="url" />
      </template>
    </contactverzoeken-zoeker>
  </div>
</template>

<script setup lang="ts">
import ContactmomentDetailsContext from "@/features/contact/contactmoment/ContactmomentDetailsContext.vue";
import ContactmomentPreview from "@/features/contact/contactmoment/ContactmomentPreview.vue";
import ContactverzoekenZoeker from "@/features/contact/contactverzoek/overzicht/ContactverzoekenZoeker.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
