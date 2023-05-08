<template>
  <UtrechtHeading :level="1">Berichten</UtrechtHeading>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else-if="error">Er is een fout opgetreden.</div>
  <ul v-else>
    <li v-for="bericht in berichten" :key="bericht.id">
      <router-link :to="'/Beheer/NieuwsEnWerkinstructie/' + bericht.id">{{
        bericht.titel
      }}</router-link>

      <utrecht-button
        appearance="secondary-action-button"
        class="icon icon-after trash icon-only"
        title="Verwijderen"
        type="button"
        @click="confirmVerwijder(bericht.id)"
      ></utrecht-button>
    </li>
  </ul>
  <menu>
    <router-link to="/Beheer/NieuwsEnWerkinstructie/">
      <utrecht-button
        appearance="primary-action-button"
        title="toevoegen"
        type="button"
        class="icon icon-after plus icon-only"
      ></utrecht-button
    ></router-link>
  </menu>
  <div v-if="deletesuccess">bericht verwijderd</div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";

type berichtType = {
  id: number;
  titel: string;
};

const loading = ref<boolean>(true);
const deletesuccess = ref<boolean>(false);
const berichten = ref<Array<berichtType>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;

  try {
    const response = await fetch("/api/berichten");
    if (response.status > 300) {
      showError();
      return;
    }
    const jsonData = await response.json();
    berichten.value = jsonData;
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;

  deletesuccess.value = false;
  try {
    const response = await fetch("/api/berichten/" + id, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
    });
    if (response.status > 300) {
      showError();
      return;
    }
    deletesuccess.value = true;
    load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("weet u zeker dat u dit item wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
</script>

<style scoped lang="scss">
menu {
  margin-block-start: var(--spacing-default);
}
li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-block: var(--spacing-small);
}
li:not(:last-child) {
  border-bottom: 2px solid var(--color-tertiary);
}
button a {
  color: white;
  text-decoration: none;
}
</style>
