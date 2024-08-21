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
            <button type="button" title="Details" @click="navigate(record)" />
          </td>
        </tr>
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import type { Persoon } from "@/services/brp";
import type { Klant } from "@/services/klanten";
import { useRouter } from "vue-router";
import { ensureKlantForBsn } from "./ensure-klant-for-bsn";
import { mutate } from "swrv";
import { watchEffect } from "vue";

const props = defineProps<{
  records: Persoon[];
  navigateOnSingleResult?: boolean;
}>();

const router = useRouter();

const getKlantUrl = (klant: Klant) => `/personen/${klant.id}`;

const navigate = async (persoon: Persoon) => {
  const { bsn, voornaam, voorvoegselAchternaam, achternaam } = persoon;
  if (!bsn) throw new Error();
  const klant = await ensureKlantForBsn({
    bsn,
    contactnaam: {
      voornaam,
      voorvoegselAchternaam,
      achternaam,
    },
  });
  await mutate("persoon" + bsn, persoon);
  await mutate(klant.id, klant);
  const url = getKlantUrl(klant);
  await router.push(url);
};

watchEffect(async () => {
  if (props.navigateOnSingleResult && props.records.length === 1) {
    await navigate(props.records[0]);
  }
});
</script>
