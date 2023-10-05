<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contactverzoeken</utrecht-heading>
    <contactverzoeken-zoeker>
      <template #onderwerp="{ contactmomentUrl }">
        <contactmoment-details-context :url="contactmomentUrl">
          <template #details="{ details }">
            {{ details.vraag }}
          </template>
        </contactmoment-details-context>
      </template>
      <template #contactmoment="{ url }">
        <contactmoment-preview :url="url">
          <template #object="{ object }">
            <zaak-preview v-if="object.object" :zaakurl="object.object" />
          </template>
        </contactmoment-preview>
      </template>
    </contactverzoeken-zoeker>
  </div>
</template>

<script setup lang="ts">
import ContactmomentDetailsContext from "@/features/contactmoment/ContactmomentDetailsContext.vue";
import ContactmomentPreview from "@/features/contactmoment/ContactmomentPreview.vue";
import ContactverzoekenZoeker from "@/features/contactverzoek/overzicht/ContactverzoekenZoeker.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
