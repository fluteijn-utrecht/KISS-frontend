<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="result.naam.loading" />
      <template v-else-if="result.naam.success">{{
        result.naam.data
      }}</template>
    </th>
    <td class="wrap">
      <div class="skeleton" v-if="result.emails.loading" />
      <template v-if="result.emails.success">{{ result.emails.data }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="result.telefoonnummers.loading" />
      <template v-if="result.telefoonnummers.success">{{
        result.telefoonnummers.data
      }}</template>
    </td>
    <td>
      {{ result.bsn }}
    </td>
    <td>
      <div class="skeleton" v-if="result.geboortedatum.loading" />
      <dutch-date
        v-else-if="result.geboortedatum.success && result.geboortedatum.data"
        :date="result.geboortedatum.data"
      />
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="result.adresregel1.loading" />
      <template v-if="result.adresregel1.success">{{
        result.adresregel1.data
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="result.adresregel2.loading" />
      <template v-if="result.adresregel2.success">{{
        result.adresregel2.data
      }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="result.detailLink.loading" />
      <template v-if="result.detailLink.success">
        <router-link
          v-if="result.detailLink.data"
          v-bind="result.detailLink.data"
        />
        <button
          type="button"
          title="Aanmaken"
          v-else
          @click="result.create()"
        />
      </template>
    </td>
  </tr>
</template>
<script lang="ts" setup>
import type { EnrichedPersoon } from "./types";
import DutchDate from "@/components/DutchDate.vue";

defineProps<{ result: EnrichedPersoon }>();
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
