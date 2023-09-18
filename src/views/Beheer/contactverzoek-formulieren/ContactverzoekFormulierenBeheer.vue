<template>
  <div class="header-wrapper">
    <UtrechtHeading :level="1">Contactverzoek formulieren</UtrechtHeading>
    <router-link to="/Beheer/Contactverzoekformulier/"> Toevoegen </router-link>
  </div>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else class="table-wrapper">
    <table class="overview">
      <thead>
        <tr>
          <th>Titel</th>
          <th>Afdeling</th>
          <th>edit/delete</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="vragenset in vragenSets" :key="vragenset.id">
          <td class="wrap">{{ vragenset.titel }}</td>
          <td>{{ vragenset.afdelingId }}</td>
          <td class="actions">
            <ul>
              <li>
                <utrecht-button
                  appearance="secondary-action-button"
                  class="icon icon-after trash icon-only"
                  :title="`Verwijder ${vragenset.Titel}`"
                  type="button"
                  @click="confirmVerwijder(vragenset.id)"
                />
              </li>
              <li>
                <router-link
                  class="icon icon-after icon-only chevron-right"
                  :to="'/Beheer/Contactverzoekformulier/' + vragenset.id"
                  :title="`Details ${vragenset.titel}`"
                />
              </li>
            </ul>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";

type VragenSets = {
  Id: number;
  Titel: string;
  JsonVragen?: string;
  AfdelingId?: string;
};
const loading = ref<boolean>(true);
const vragenSets = ref<Array<VragenSets>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;
  try {
    const response = await fetchLoggedIn("/api/contactverzoekvragensets");
    if (response.status > 300) {
      showError();
      return;
    }
    const jsonData: any[] = await response.json();
    vragenSets.value = jsonData
      .map((x) => ({
        ...x,
        Id: Number(x.id),
        dateCreated: new Date(x.dateCreated),
        dateUpdated: x.dateUpdated && new Date(x.dateUpdated),
        publicatiedatum: new Date(x.publicatiedatum),
      }))
      .sort((a, b) => b.Id - a.Id);
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;

  try {
    const response = await fetchLoggedIn(
      "/api/contactverzoekvragensets/" + id,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      },
    );
    if (response.status > 300) {
      showError();
      return;
    }
    toast({ text: "Contactformulier verwijderd" });
    await load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u dit contactformulier wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
</script>

<style scoped lang="scss">
.actions {
  vertical-align: middle;

  ul {
    display: flex;
    gap: var(--spacing-small);
  }

  li {
    display: flex;
    align-items: stretch;
    width: fit-content;
  }

  a {
    display: flex;
    inline-size: 2rem;
  }
}
</style>
