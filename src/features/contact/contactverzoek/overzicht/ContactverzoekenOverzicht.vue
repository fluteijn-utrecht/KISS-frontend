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
          <span v-if="contactverzoek.onderwerp"
            >{{ contactverzoek.onderwerp.slice(0, 18)
            }}<template v-if="contactverzoek.onderwerp.length > 18">
              ...</template
            >
          </span>
          <span v-else>
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

          <!-- 
            Voor OK1/esuite worden een aantal gegevens uit het contactmometn gehaald.
            Deze moeten apart worden opgehaald
          -->
          <slot
            name="contactmoment"
            :url="contactverzoek.record.data.contactmoment"
          ></slot>
          <!--
            voor OK2 zijn deze gegevens reeds in het contactverzoek beschikbaar.
            todo: voor zaken moet hier tzt nog iets geregeld worden
          -->
          <template v-if="contactverzoek.medewerker">
            <dt>Aangemaakt door</dt>
            <dd>{{ contactverzoek.medewerker }}</dd>
            <dt>Vraag</dt>
            <dd>{{ contactverzoek.onderwerp }}</dd>
            <dt>Toelichting</dt>
            <dd>{{ contactverzoek.toelichting }}</dd>
          </template>
        </dl>
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { fullName } from "@/helpers/string";
import ExpandableTableList from "@/components/ExpandableTableList.vue";
import type { Contactverzoek } from "./types";

defineProps<{
  contactverzoeken: Contactverzoek[];
}>();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;
</script>

<style scoped>
.preserve-newline {
  white-space: pre-line;
}
</style>
