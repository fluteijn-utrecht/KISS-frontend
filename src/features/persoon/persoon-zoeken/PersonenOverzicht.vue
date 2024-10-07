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
import { useRouter } from "vue-router";
import { useOrganisatieIds } from "@/stores/user";
import { mutate } from "swrv";
import { watchEffect } from "vue";

import { ensureKlantForBsn as ensureKlantForBsn1 } from "@/services/openklant1/service";
import { ensureKlantForBsn as ensureKlantForBsn2, useOpenKlant2 } from "@/services/openklant2/service";
import type { Klant as Klant1 } from "@/services/openklant1/types";
import type { Klant as Klant2 } from "@/services/openklant2/types";

const props = defineProps<{
  records: Persoon[];
  navigateOnSingleResult?: boolean;
}>();

const router = useRouter();

const isOpenKlant2 = await useOpenKlant2();

type Klant = Klant1 | Klant2;

const getKlantUrl = (klant: Klant) => `/personen/${klant.id}`;

const navigate = async (persoon: Persoon) => {
  const { bsn } = persoon;
  if (!bsn) throw new Error("BSN is required");

  let klant: Klant;

  if (isOpenKlant2) {
    // Gebruik openklant2 implementatie
    klant = await ensureKlantForBsn2(bsn);
  } else {
    // Gebruik openklant1 implementatie
    const organisatieIds = useOrganisatieIds();
    klant = (await ensureKlantForBsn1({ bsn }, organisatieIds.value[0] || ""));
  }

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