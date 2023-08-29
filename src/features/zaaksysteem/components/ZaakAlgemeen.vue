<template>
  <utrecht-heading :level="2" modelValue>Algemeen</utrecht-heading>
  <div>
    <dl>
      <dt>Titel</dt>
      <dd>{{ zaak.omschrijving }}</dd>
      <dt>Zaaktype</dt>
      <dd>{{ zaak.zaaktypeOmschrijving }}</dd>
      <dt>Zaaknummer</dt>
      <dd>{{ zaak.identificatie }}</dd>
      <dt>Indiendatum</dt>
      <dd>
        <time-or-onbekend class="value" :date="zaak.startdatum" />
      </dd>
    </dl>
    <dl>
      <dt>Aanvrager</dt>
      <dd>{{ zaak.aanvrager }}</dd>
      <dt>Status</dt>
      <dd>{{ zaak.status }}</dd>
      <dt>Behandelaar</dt>
      <dd>{{ zaak.behandelaar }}</dd>
      <!-- <dt>Registratiedatum</dt>
      <dd>
        <time-or-onbekend class="value" :date="zaak.registratieDatum" />
      </dd>
      <dt>Startdatum</dt>
      <dd>
        <time-or-onbekend class="value" :date="zaak.startdatum" />
      </dd>
      <dt>Streefdatum</dt>
      <dd>
        <time-or-onbekend class="value" :date="zaak.streefDatum" />
      </dd>
      <dt>Fatale datum</dt>
      <dd>
        <time-or-onbekend class="value" :date="zaak.fataleDatum" />
      </dd> -->
      <dt>Toelichting</dt>
      <dd>{{ zaak.toelichting }}</dd>
    </dl>
  </div>
</template>

<script setup lang="ts">
import type { ZaakDetails } from "./../types";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import TimeOrOnbekend from "./TimeOrOnbekend.vue";

defineProps<{
  zaak: ZaakDetails;
}>();
</script>

<style scoped lang="scss">
div {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8rem;
  margin-block-start: var(--spacing-default);
}

dl {
  block-size: max-content;
  display: grid;
  grid-template-columns: auto auto;
  column-gap: var(--spacing-large);
  row-gap: var(--line-height-default);
}
</style>
