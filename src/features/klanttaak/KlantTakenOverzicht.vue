<template>
  <section>
    <expandable-table-list :items="taken" item-key="url">
      <template #header>
        <span id="titel-header">Titel</span>
        <span id="status-header">Status</span>
        <span id="verloopdatum-header">Verloopdatum</span>
      </template>

      <template v-slot:item="{ item: taak }">
        <summary>
          <span>{{ taak.record.data.title }}</span>
          <span>{{ taak.record.data.status }}</span>
          <dutch-date
            v-if="taak.record.data.verloopdatum"
            :date="new Date(taak.record.data.verloopdatum)"
          />
          <span v-else />
        </summary>
        <dl v-if="taak.record.data.data">
          <template
            v-for="[key, value] in Object.entries(taak.record.data.data)"
            :key="key"
          >
            <dt>
              {{ capitalizeFirstLetter(key) }}
            </dt>
            <dd>
              {{ value }}
            </dd>
          </template>
        </dl>
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import ExpandableTableList from "@/components/ExpandableTableList.vue";
import type { KlantTaak } from "./types";
import DutchDate from "@/components/DutchDate.vue";
import { capitalizeFirstLetter } from "@/helpers/string";
defineProps<{
  taken: KlantTaak[];
}>();
</script>
