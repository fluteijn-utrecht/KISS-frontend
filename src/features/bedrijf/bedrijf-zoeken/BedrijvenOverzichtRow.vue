<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-else-if="bedrijf.success">{{
        bedrijf.data?.bedrijfsnaam
      }}</template>
    </th>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{ bedrijf.data?.type }}</template>
    </td>
    <td>
      {{ item.kvkNummer }}
    </td>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{
        [bedrijf.data?.postcode, bedrijf.data?.huisnummer].join(" ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="klant.loading" />
      <template v-if="klant.success">{{
        klant.data?.emailadressen?.join(", ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="klant.loading" />
      <template v-if="klant.success">{{
        klant.data?.telefoonnummers.join(", ")
      }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="klant.loading || bedrijf.loading" />
      <template v-if="klant.success">
        <router-link
          v-if="klant.data"
          :title="`Details ${naam}`"
          :to="getKlantUrl(klant.data)"
          @click="setCache(klant.data)"
        />
        <button
          type="button"
          title="Aanmaken"
          v-else-if="parameterForEnsureKlant"
          @click="create(parameterForEnsureKlant)"
        />
      </template>
    </td>
  </tr>
</template>
<script lang="ts" setup>
import { computed } from "vue";
import type { Klant } from "../../../services/klanten/types";
import { useBedrijfByIdentifier } from "../bedrijf-details/use-bedrijf-by-identifier";
import { useKlantByBedrijfIdentifier } from "./use-klant-by-bedrijf-identifier";
import { mutate } from "swrv";
import type { Bedrijf, BedrijfIdentifier } from "@/services/kvk";
import { ensureKlantForBedrijfIdentifier } from "./ensure-klant-for-bedrijf-identifier";

const props = defineProps<{ item: Bedrijf | Klant }>();

const matchingBedrijf = useBedrijfByIdentifier(() => {
  // we hebben al een bedrijf, we hoeven die niet meer op te zoeken
  if (props.item._typeOfKlant === "bedrijf") return undefined;
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

const matchingKlant = useKlantByBedrijfIdentifier(() => {
  // we hebben al een klant, we hoeven die niet meer op te zoeken
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
    : { ...matchingBedrijf },
);

const klant = computed(() =>
  props.item._typeOfKlant === "klant"
    ? { data: props.item, success: true, loading: false, error: false }
    : { ...matchingKlant },
);

const naam = computed(() =>
  props.item._typeOfKlant === "bedrijf"
    ? props.item.bedrijfsnaam
    : matchingBedrijf.success && matchingBedrijf.data?.bedrijfsnaam
      ? matchingBedrijf.data.bedrijfsnaam
      : props.item.bedrijfsnaam,
);

const getKlantUrl = (klant: Klant) => `/bedrijven/${klant.id}`;

const setCache = (klant: Klant) => {
  mutate(klant.id, klant);
  const bedrijfId =
    bedrijf.value.data?.vestigingsnummer || bedrijf.value.data?.rsin;
  if (bedrijfId) {
    mutate("bedrijf" + bedrijfId, bedrijf.value.data);
  }
};

const parameterForEnsureKlant = computed(() => {
  if (!bedrijf.value?.data?.bedrijfsnaam) return undefined;
  const {
    bedrijfsnaam: naam,
    vestigingsnummer,
    rsin,
    kvkNummer,
  } = bedrijf.value.data;
  if (vestigingsnummer)
    return {
      vestigingsnummer,
      naam,
    };
  if (rsin)
    return {
      rsin,
      kvkNummer,
      naam,
    };
  return undefined;
});

async function create(parameter: BedrijfIdentifier & { naam: string }) {
  const klant = await ensureKlantForBedrijfIdentifier(parameter);
  setCache(klant);
}
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
