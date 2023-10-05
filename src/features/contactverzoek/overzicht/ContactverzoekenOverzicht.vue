<template>
  <section>
    <expandable-table-list :items="contactverzoeken" item-key="url">
      <template #header>
        <span id="datum-header" class="icon-after sort-descending">Datum</span>
        <span id="onderwerp-header">Onderwerp</span>
        <span id="medewerker-header">Status</span>
        <span id="kanaal-header">Behandelaar</span>
      </template>

      <template
        v-slot:item="{ item: contactverzoek }: { item: Contactverzoek }"
      >
        <summary>
          <dutch-date
            v-if="contactverzoek.record.data.registratiedatum"
            :date="new Date(contactverzoek.record.data.registratiedatum)"
          />
          <span v-else />
          <span>
            <slot
              name="onderwerp"
              :contactmoment-url="contactverzoek.record.data.contactmoment"
            >
            </slot>
          </span>
          <span>{{ contactverzoek.record.data.status }}</span>
          <span>{{ contactverzoek.record.data.actor.naam }}</span>
        </summary>
        <dl>
          <dt>Starttijd</dt>
          <dd>
            <dutch-time
              v-if="contactverzoek.record.data.registratiedatum"
              :date="new Date(contactverzoek.record.data.registratiedatum)"
            />
          </dd>

          <dt>Toelichting voor de collega</dt>
          <dd class="preserve-newline">
            {{ contactverzoek.record.data.toelichting }}
          </dd>

          <template
            v-if="
              contactverzoek.record.data.betrokkene.persoonsnaam?.achternaam
            "
          >
            <dt>Naam betrokkene</dt>
            <dd>
              {{ fullName(contactverzoek.record.data.betrokkene.persoonsnaam) }}
            </dd>
          </template>

          <template v-if="contactverzoek.record.data.betrokkene.organisatie">
            <dt>Organisatie</dt>
            <dd>{{ contactverzoek.record.data.betrokkene.organisatie }}</dd>
          </template>

          <template
            v-for="(adres, idx) in contactverzoek.record.data.betrokkene
              .digitaleAdressen"
            :key="idx"
          >
            <dt>
              {{
                capitalizeFirstLetter(
                  adres.omschrijving || adres.soortDigitaalAdres || "contact",
                )
              }}
            </dt>
            <dd>
              {{ adres.adres }}
            </dd>
          </template>

          <slot
            name="contactmoment"
            :url="contactverzoek.record.data.contactmoment"
          ></slot>
        </dl>
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { watch } from "vue";
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { fullName } from "@/helpers/string";
import ExpandableTableList from "@/components/ExpandableTableList.vue";
import type { Contactverzoek } from "../types";

const props = defineProps<{
  contactverzoeken: Contactverzoek[];
}>();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;

const activeContactverzoeken = ref([] as boolean[]);

watch(
  () => props.contactverzoeken.length - activeContactverzoeken.value.length,
  (diff) => {
    for (let index = 0; index < diff; index++) {
      activeContactverzoeken.value.push(false);
    }
  },
);
</script>

<style scoped>
.preserve-newline {
  white-space: pre-line;
}
</style>
