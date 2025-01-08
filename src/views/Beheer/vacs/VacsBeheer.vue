<template>
  <utrecht-heading :level="1">Vacs</utrecht-heading>

  <div v-if="loading"><SimpleSpinner /></div>

  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li v-for="vac in vacs" :key="vac.uuid" class="listItem">
      <router-link :to="'/Beheer/vac/' + vac.uuid">{{ vac.vraag }}</router-link>

      <utrecht-button
        appearance="secondary-action-button"
        class="icon icon-after trash icon-only"
        title="Verwijderen"
        type="button"
      ></utrecht-button>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn, parseJson, parsePagination } from "@/services";
import type { Vac } from "@/features/search/types";

const vacs = ref<Vac[]>();

const loading = ref<boolean>(true);
const error = ref<boolean>(false);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapVac = (x: any): Vac => ({
  uuid: x.uuid,
  vraag: x.record.data.vraag,
  antwoord: x.record.data.antwoord,
});

const fetchAllVacs = async (url: string): Promise<Vac[]> => {
  const { page, next } = await fetchLoggedIn(url)
    .then(parseJson)
    .then((json) => parsePagination(json, mapVac));

  if (next) {
    const { searchParams } = new URL(next);

    return [
      ...page,
      ...(await fetchAllVacs(`${url}?page=${searchParams.get("page")}`)),
    ];
  }

  return page;
};

const load = async () => {
  loading.value = true;

  fetchAllVacs("/api/vacs/api/v2/objects")
    .then((results) => (vacs.value = results))
    .catch(() => (error.value = true))
    .finally(() => (loading.value = false));
};

onMounted(() => load());
</script>
