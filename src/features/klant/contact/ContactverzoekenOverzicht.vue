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

    <!-- 
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Tel. nummer</th>
          <th>E-mailadres</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(record, i) in records" :key="i" class="row-link">
          <td>
            <pre>{{ record }}</pre>
          </td>
          <td>{{ record.emailadres }}</td>
          <td>
            <router-link
              :to="`/contactverzoeken/${record.uuid}`"
              title="Details"
            />
          </td>
        </tr>
      </tbody>
    </template> -->
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
