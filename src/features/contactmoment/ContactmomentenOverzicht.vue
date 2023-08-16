<template>
  <!-- TODO this is semantically weird: a list pretending to be a table -->
  <!-- Unless the user group has a very good reason, I would rather just make it a list of <dl>s with no fake header row -->
  <section>
    <template v-if="contactmomenten.length">
      <ul>
        <li class="header-row">
          <span id="datum-header">Datum</span>
          <span id="medewerker-header">Medewerker</span>
          <span id="kanaal-header">Kanaal</span>
          <span id="gespreksresultaat-header">Gespreksresultaat</span>
        </li>
        <li v-for="contactmoment in contactmomenten" :key="contactmoment.url">
          <ContactmomentenOverzichtItem :contactmoment="contactmoment">
            <template #object="{ object }">
              <slot name="object" :object="object"></slot>
            </template>
          </ContactmomentenOverzichtItem>
        </li>
      </ul>
    </template>

    <span v-else>Geen contactmomenten gevonden.</span>
  </section>
</template>

<script lang="ts" setup>
import type { ContactmomentViewModel } from "../shared/types";
import ContactmomentenOverzichtItem from "./ContactmomentenOverzichtItem.vue";

defineProps<{
  contactmomenten: ContactmomentViewModel[];
}>();
</script>

<style lang="scss" scoped>
ul {
  --column-width: 25ch;
  --gap: var(--spacing-default);
  --columns: 1fr 1fr 1fr 1fr 1rem;

  display: grid;
  list-style: none;
  padding: 0;
}

li:not(:first-child, :last-child) {
  border-bottom: 2px solid var(--color-tertiary);
}

.header-row {
  display: grid;
  grid-template-columns: var(--columns);
  gap: var(--gap);
  padding-inline: var(--gap);
  padding-block: var(--spacing-default);
  background: var(--color-tertiary);
  color: var(--color-white);
}
</style>
