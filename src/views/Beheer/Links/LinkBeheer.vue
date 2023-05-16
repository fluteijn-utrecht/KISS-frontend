<template>
  <utrecht-heading :level="1">Link</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="link">
    <form class="container" @submit.prevent="submit">
      <label for="titel" class="utrecht-form-label">
        <span>Titel</span>
        <input type="text" id="titel" v-model="link.titel" required />
      </label>

      <label for="naam" class="utrecht-form-label">
        <span>Url</span>
        <input
          type="url"
          id="url"
          v-model="link.url"
          required
          pattern="https://.+"
          :title="'Een url moet beginnen met https://'"
        />
      </label>

      <label for="categorie" class="utrecht-form-label">
        <span>Categorie</span>
        <SimpleTypeahead
          v-model="link.categorie"
          :defaultItem="link.categorie"
          required
          id="categorie"
          :items="categorien"
          :minInputLength="1"
          @selectItem="(e:any) => { if(link){link.categorie = e;}}"
        >
        </SimpleTypeahead>
      </label>

      <menu>
        <li>
          <router-link to="/Beheer/links/">
            <utrecht-button appearance="secondary-action-button" type="button">
              Annuleren
            </utrecht-button>
          </router-link>
        </li>

        <li>
          <utrecht-button appearance="primary-action-button" type="submit">
            Opslaan
          </utrecht-button>
        </li>
      </menu>
    </form>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { useRouter } from "vue-router";
import SimpleTypeahead from "vue3-simple-typeahead";

const props = defineProps<{ id: string }>();

type Link = {
  id?: number;
  titel?: string;
  url?: string;
  categorie?: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const link = ref<Link | null>(null);

const categorien = ref<Array<string>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "link opgeslagen",
  });
  return router.push("/Beheer/links/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      const result = await fetchLoggedIn("/api/links/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/links/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    }
    return handleSuccess();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  loading.value = true;

  try {
    if (props.id) {
      //load link
      const response = await fetchLoggedIn("/api/links/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();

      link.value = jsonData;
    } else {
      link.value = {};
    }

    //load categorie suggestions
    const categoriesResponse = await fetchLoggedIn("/api/categorien");
    if (categoriesResponse.status == 200) {
      categorien.value = await categoriesResponse.json();
    }
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
});
</script>

<style>
menu {
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

form {
  margin-top: var(--spacing-default);
}

label > span {
  display: block;
}

.simple-typeahead-list {
  background-color: var(--color-secondary);
}

.simple-typeahead-list-item {
  padding: var(--spacing-small);
}

.simple-typeahead-list-item:hover,
.simple-typeahead-list-item-active {
  background-color: var(--color-primary-hover);
}
</style>
