<template>
  <article class="details-block">
    <utrecht-heading :level="2"> Gegevens BRP</utrecht-heading>
    <dl>
      <dt>Naam</dt>
      <dd>
        {{
          [persoon.voornaam, persoon.voorvoegselAchternaam, persoon.achternaam]
            .filter(Boolean)
            .join(" ")
        }}
      </dd>
      <dt>Bsn</dt>
      <dd>{{ persoon.bsn }}</dd>
      <dt>Geboortedatum</dt>
      <dd>
        <dutch-date
          v-if="persoon.geboortedatum"
          :date="persoon.geboortedatum"
        />
      </dd>
      <dt>Geboorteplaats</dt>
      <dd>{{ persoon.geboorteplaats }}</dd>
      <dt>Geboorteland</dt>
      <dd>{{ persoon.geboorteland }}</dd>
      <template v-if="persoon.adresregel1">
        <dt>Adresregel 1</dt>
        <dd>{{ persoon.adresregel1 }}</dd>
      </template>
      <template v-if="persoon.adresregel2">
        <dt>Adresregel 2</dt>
        <dd>{{ persoon.adresregel2 }}</dd>
      </template>
      <template v-if="persoon.adresregel3">
        <dt>Adresregel 3</dt>
        <dd>{{ persoon.adresregel3 }}</dd>
      </template>
    </dl>
  </article>
</template>

<script setup lang="ts">
import type { Persoon } from "@/services/brp";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DutchDate from "@/components/DutchDate.vue";

defineProps<{ persoon: Persoon }>();
</script>
