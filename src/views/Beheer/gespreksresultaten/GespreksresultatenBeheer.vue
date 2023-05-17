<template>
  <utrecht-heading :level="1">Gespreksresultaten</utrecht-heading>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li
      v-for="{ id, definitie: titel } in gespreksresultaten"
      :key="id"
      class="listItem"
    >
      <router-link :to="BEHEER_URL + id">{{ titel }}</router-link>

      <utrecht-button
        appearance="secondary-action-button"
        class="icon icon-after trash icon-only"
        title="Verwijderen"
        type="button"
        @click="confirmVerwijder(id)"
      ></utrecht-button>
    </li>
  </ul>
  <menu>
    <router-link :to="BEHEER_URL">
      <utrecht-button
        appearance="primary-action-button"
        title="toevoegen"
        type="button"
        class="icon icon-after plus icon-only"
      >
      </utrecht-button>
    </router-link>
  </menu>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn, throwIfNotOk } from "@/services";
import { parseJson } from "@/services";

type Gespreksresultaat = {
  id: number;
  definitie: string;
};
const API_URL = "/api/gespreksresultaten/";
const BEHEER_URL = "/beheer/gespreksresultaat/";

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
const gespreksresultaten = ref<Array<Gespreksresultaat>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

function withState<T>(p: Promise<T>) {
  loading.value = true;
  return p.catch(showError).finally(() => {
    loading.value = false;
  });
}

const load = () =>
  withState(
    fetchLoggedIn(API_URL)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) => {
        gespreksresultaten.value = r;
      })
  );

const verwijder = (id: number) =>
  withState(
    fetchLoggedIn(API_URL + id, {
      method: "DELETE",
    })
      .then(throwIfNotOk)
      .then(load)
      .then(() =>
        toast({
          text: "Gespreksresultaat verwijderd",
        })
      )
  );

const confirmVerwijder = (id: number) => {
  if (confirm("weet u zeker dat u dit gespreksresultaat wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(load);
</script>
