<template>
  <section>
    <expandable-table-list :items="notificaties" item-key="url">
      <template #header>
        <span id="datum-header" class="icon-after sort-descending">Datum</span>
        <span id="kanaal-header">Kanaal</span>
      </template>

      <template v-slot:item="{ item }">
        <NotificatiesOverzichtItem :contactmoment="item">
          <template #object="{ object }">
            <slot name="object" :object="object"></slot>
          </template>
        </NotificatiesOverzichtItem>
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import type { ContactmomentViewModel } from "../shared/types";
import NotificatiesOverzichtItem from "./NotificatiesOverzichtItem.vue";
import ExpandableTableList from "@/components/ExpandableTableList.vue";

defineProps<{
  notificaties: ContactmomentViewModel[]; //een notificatie is technisch gezien een contactmoment
}>();
</script>
