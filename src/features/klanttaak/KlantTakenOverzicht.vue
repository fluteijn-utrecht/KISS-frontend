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
          <time
            v-if="taak.record.data.verloopdatum"
            :datetime="new Date(taak.record.data.verloopdatum).toISOString()"
          >
            {{ formatDateAndTime(taak.record.data.verloopdatum) }}
          </time>
          <span v-else />
        </summary>
        <dl v-if="taak.record.data.data">
          <template
            v-for="[key, value] in Object.entries(taak.record.data.data)"
            :key="key"
          >
            <dt>
              {{ camelCaseToSentence(key) }}
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
import { camelCaseToSentence } from "@/helpers/string";
import { formatDateAndTime } from "@/helpers/date";
defineProps<{
  taken: KlantTaak[];
}>();
</script>
