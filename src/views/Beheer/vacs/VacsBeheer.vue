<template>
  <utrecht-heading :level="1">Vacs</utrecht-heading>

  <div v-if="loading"><SimpleSpinner /></div>

  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li v-for="vac in result?.page" :key="vac.uuid" class="listItem">
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
import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";
import type { Vac } from "@/features/search/types";

const result = ref<PaginatedResult<Vac>>();

const loading = ref<boolean>(true);
const error = ref<boolean>(false);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapVac = (x: any): Vac => ({
  uuid: x.uuid,
  vraag: x.record.data.vraag,
  antwoord: x.record.data.antwoord,
});

const load = async () => {
  loading.value = true;

  result.value = await fetchLoggedIn("/api/vacs/api/v2/objects")
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapVac))
    .finally(() => (loading.value = false));
};

onMounted(() => load());
</script>

<style scoped lang="scss">
li h2 {
  margin-block-start: var(--spacing-large);
}
</style>
