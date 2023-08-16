<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Tel. nummer</th>
          <th>E-mailadres</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(klant, i) in records" :key="i" class="row-link">
          <td>{{ klant.telefoonnummer }}</td>
          <td>{{ klant.emailadres }}</td>
          <td>
            <router-link :to="`/contacten/${klant.id}`" title="Details" />
          </td>
        </tr>
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import type { Klant } from "../types";
defineProps<{ records: Klant[] }>();
</script>
