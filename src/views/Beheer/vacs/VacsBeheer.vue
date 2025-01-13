<template>
  <utrecht-heading :level="1">Vacs</utrecht-heading>

  <simple-spinner v-if="loading" />

  <div v-else-if="error">
    Er is een fout opgetreden bij het ophalen van de Vacs.
  </div>

  <ul v-else>
    <li v-for="vac in vacs" :key="vac.uuid" class="listItem">
      <router-link :to="'/Beheer/vac/' + vac.uuid">{{ vac.vraag }}</router-link>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn, parseJson, parsePagination } from "@/services";
import type { Vac } from "@/features/search/types";

const vacs = ref<Vac[]>();
const loading = ref(false);
const error = ref(false);

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
    const [pathname] = url.split("?");
    const { searchParams } = new URL(next);

    return [
      ...page,
      ...(await fetchAllVacs(`${pathname}?page=${searchParams.get("page")}`)),
    ];
  }

  return page;
};

onMounted(() => {
  loading.value = true;

  fetchAllVacs("/api/vacs/api/v2/objects")
    .then(
      (results) =>
        (vacs.value = results.sort((a, b) =>
          a.vraag ? a.vraag.localeCompare(b.vraag) : -1,
        )),
    )
    .catch(() => error.value)
    .finally(() => (loading.value = false));
});
</script>
