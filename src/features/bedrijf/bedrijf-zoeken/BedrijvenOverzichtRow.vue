<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="bedrijf.loading" />

      <template v-else-if="bedrijf.success">
        {{ bedrijf.data?.bedrijfsnaam }}</template
      >
    </th>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{ bedrijf.data?.type }}</template>
    </td>
    <td>
      {{ bedrijf.data?.kvkNummer }}
    </td>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{
        [bedrijf.data?.postcode, bedrijf.data?.huisnummer].join(" ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">{{
        matchingKlant.data?.emailadressen?.join(", ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">{{
        matchingKlant.data?.telefoonnummers.join(", ")
      }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="matchingKlant.loading || bedrijf.loading" />

      <template v-if="matchingKlant.success && matchingKlant.data">
        <router-link
          :title="`Details ${naam}`"
          :to="getKlantUrl(matchingKlant.data)"
          @click="setCache(matchingKlant.data, bedrijf.data)"
        />
      </template>
      <button
        v-else-if="bedrijf.data && bedrijfIdentifier"
        type="button"
        title="Aanmaken"
        @click="navigate(bedrijf.data, bedrijfIdentifier)"
      />
    </td>
  </tr>
</template>
<script lang="ts" setup>
import { computed, watchEffect } from "vue";
import { useBedrijfByIdentifier } from "../use-bedrijf-by-identifier";
import { useKlantByBedrijfIdentifier } from "./use-klant-by-bedrijf-identifier";
import type { Bedrijf, BedrijfIdentifier } from "@/services/kvk";
import { useRouter } from "vue-router";
import { mutate } from "swrv";
import { ensureKlantForBedrijfIdentifier } from "./ensure-klant-for-bedrijf-identifier";
import type { Klant as KlantOpenKlant1 } from "@/services/openklant1/types";
import type { Klant as KlantOpenKlant2 } from "@/services/openklant2/types";

const props = defineProps<{
  item: Bedrijf | KlantOpenKlant1 | KlantOpenKlant2;
  autoNavigate?: boolean;
}>();

// const matchingBedrijf = useBedrijfByIdentifier(() => {
//   // wordt niet meer gebruikt, alleen als we een klant hebben maar telefoonnumer en email adres is eruitgesloopt
//   if (props.item._typeOfKlant === "bedrijf") return undefined;
//   const { vestigingsnummer, rsin } = props.item;
//   if (vestigingsnummer)
//     return {
//       vestigingsnummer,
//     };
//   if (rsin)
//     return {
//       rsin,
//     };
// });

const matchingKlant = useKlantByBedrijfIdentifier(() => {
  if (props.item._typeOfKlant === "klant") return undefined;
  const { vestigingsnummer, rsin } = props.item;

  if (vestigingsnummer)
    return {
      vestigingsnummer,
    };
  if (rsin)
    return {
      rsin,
    };
});

const bedrijf = computed(() =>
  props.item._typeOfKlant === "bedrijf"
    ? { data: props.item, success: true, loading: false, error: false }
    : { success: false, loading: false },
);

// const klant = computed(() => {
//   console.log(props.item._typeOfKlant === "klant", { ...matchingKlant });

//   const x =
//     props.item._typeOfKlant === "klant"
//       ? { data: props.item, success: true, loading: false, error: false }
//       : { ...matchingKlant };

//   return x;
// });

const naam = computed(() => bedrijf.value.data?.bedrijfsnaam || "");

const bedrijfIdentifier = computed<BedrijfIdentifier | undefined>(() => {
  const { rsin, kvkNummer, vestigingsnummer } = bedrijf.value.data ?? {};
  if (vestigingsnummer)
    return {
      vestigingsnummer,
    };
  if (rsin)
    return {
      rsin,
      kvkNummer,
    };
  return undefined;
});

const router = useRouter();

const getKlantUrl = (klant: KlantOpenKlant1 | KlantOpenKlant2) =>
  `/bedrijven/${klant.id}`;

const setCache = (
  klant: KlantOpenKlant1 | KlantOpenKlant2,
  bedrijf?: Bedrijf | null,
) => {
  mutate(klant.id, klant);
  const bedrijfId = bedrijf?.vestigingsnummer || bedrijf?.rsin;
  if (bedrijfId) {
    mutate("bedrijf" + bedrijfId, bedrijf);
  }
};

async function navigate(bedrijf: Bedrijf, identifier: BedrijfIdentifier) {
  const bedrijfsnaam = bedrijf.bedrijfsnaam;
  const klant = await ensureKlantForBedrijfIdentifier(identifier, bedrijfsnaam);

  setCache(klant, bedrijf);

  const url = getKlantUrl(klant);
  await router.push(url);
}

watchEffect(() => {
  if (
    props.autoNavigate &&
    matchingKlant.success &&
    bedrijf.value.data &&
    bedrijfIdentifier.value
  ) {
    navigate(bedrijf.value.data, bedrijfIdentifier.value);
  }
});
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
