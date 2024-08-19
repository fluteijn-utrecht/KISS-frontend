<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Naam</th>
          <th>Geboortedatum</th>
          <th>Adresregel 1</th>
          <th>Adresregel 2</th>
          <th>Bsn</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <tr class="row-link" v-for="(record, idx) in records" :key="idx">
          <th scope="row" class="wrap">
            {{
              [record.voornaam, record.voorvoegselAchternaam, record.achternaam]
                .filter(Boolean)
                .join(" ")
            }}
          </th>
          <td>
            <dutch-date
              v-if="record.geboortedatum"
              :date="record.geboortedatum"
            />
          </td>
          <td class="wrap">
            {{ record.adresregel1 }}
          </td>
          <td class="wrap">
            {{ record.adresregel2 }}
          </td>
          <td>
            {{ record.bsn }}
          </td>

          <td>
            <button type="button" title="Details" @click="create(record)" />
          </td>
        </tr>
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import type { Persoon } from "./types";
import type { Klant } from "../types";
import { useRouter } from "vue-router";
import { ensureKlantForBsn } from "./ensure-klant-for-bsn";

defineProps<{ records: Persoon[] }>();

const router = useRouter();

const getKlantUrl = (klant: Klant) => `/personen/${klant.id}`;

const create = async ({
  bsn,
  voornaam,
  voorvoegselAchternaam,
  achternaam,
}: Persoon) => {
  if (!bsn) throw new Error();
  const newKlant = await ensureKlantForBsn({
    bsn,
    contactnaam: {
      voornaam,
      voorvoegselAchternaam,
      achternaam,
    },
  });
  const url = getKlantUrl(newKlant);
  router.push(url);
};
</script>
