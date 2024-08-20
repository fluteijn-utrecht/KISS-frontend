<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Naam</th>
          <th>Type</th>
          <th>KvK-nummer</th>
          <th class="wrap">Postcode + Huisnummer</th>
          <th>E-mailadres</th>
          <th>Tel. nummer</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <bedrijven-overzicht-row
          v-for="(record, idx) in records"
          :item="record"
          :key="idx"
        />
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import type { Bedrijf } from "@/services/kvk";
import BedrijvenOverzichtRow from "./BedrijvenOverzichtRow.vue";
import type { Klant } from "@/services/klanten";

defineProps<{ records: Array<Bedrijf | Klant> }>();
</script>
